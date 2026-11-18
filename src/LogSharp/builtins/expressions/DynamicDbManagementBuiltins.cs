using System;
using System.Collections.Generic;
using System.Linq;
using logSharp.helpers;
using logSharp.interpretation;
using logSharp.interpretation.unification;
using logSharp.model;

namespace logSharp.builtins.expressions
{
    /// <summary>
    /// Dynamic database management (assert/retract) : lets a program mutate its
    /// own knowledge base at runtime.
    ///
    /// - assertz/1 appends a clause at the end of its predicate (ISO assert/1)
    /// - asserta/1 prepends it, so it is evaluated FIRST by subsequent calls
    /// - assert/1 evaluates bare expressions (the parser wraps "X > 0" style
    ///   goals into assert/1) and behaves like assertz/1 for clause terms
    /// - retract/1 removes the clauses unifying with its pattern, one solution
    ///   per clause, and is resumable through backtracking
    /// - retractall/1 removes every clause unifying with its pattern and always
    ///   succeeds
    ///
    /// Mutation safety : the clause store is only ever enumerated through
    /// snapshots (copy-on-read), and retract unlinks the clause from every
    /// table (ordered lists, fallback list, hash buckets) in one step. Choice
    /// points active in the middle of a modified predicate keep iterating the
    /// snapshot they captured; they are never corrupted by the structural
    /// mutation.
    /// </summary>
    [BuiltIns]
    public class DynamicDbManagementBuiltins
    {

        public const string AssertaBuiltinName = "asserta";
        public const string AssertzBuiltinName = "assertz";
        public const string RetractBuiltinName = "retract";
        public const string RetractAllBuiltinName = "retractall";

        // the ":-" constructor, used to build/retrieve rule clauses
        public const string ImplyFunctor = ":-";
        private const string ConjunctionFunctor = ",";
        private const string TrueAtomName = "true";

        [BuiltIn("asserta/1")]
        public static List<Unification> AssertAPrim(Interpreter interpreter, Goal goal,
            List<Valuable> parameters)
        {
            return AssertClausePrim(interpreter, parameters, atEnd: false);
        }

        [BuiltIn("assertz/1")]
        public static List<Unification> AssertZPrim(Interpreter interpreter, Goal goal,
            List<Valuable> parameters)
        {
            return AssertClausePrim(interpreter, parameters, atEnd: true);
        }

        [BuiltIn("retract/1")]
        public static List<Unification> RetractPrim(Interpreter interpreter, Goal goal,
            List<Valuable> parameters)
        {
            List<Unification> unifications = Retract(interpreter, goal, parameters, exportBindings: true);
            if (unifications == null || !unifications.Any())
            {
                // invalid pattern, or nothing matched : retract fails
                return ListHelper.BuildList(Unification.Failed());
            }

            return unifications;
        }

        [BuiltIn("retractall/1")]
        public static List<Unification> RetractAllPrim(Interpreter interpreter, Goal goal,
            List<Valuable> parameters)
        {
            List<Unification> unifications = Retract(interpreter, goal, parameters, exportBindings: false);
            if (unifications == null)
            {
                // invalid (unbound or non callable) pattern
                return ListHelper.BuildList(Unification.Failed());
            }

            // retractall always succeeds, even when no clause was removed
            return ListHelper.BuildList(Unification.Success());
        }

        // ---------------------------------------------------------------------
        // assertion

        private static List<Unification> AssertClausePrim(Interpreter interpreter,
            List<Valuable> parameters, bool atEnd)
        {
            if (parameters.Count != 1)
            {
                return ListHelper.BuildList(Unification.Failed());
            }

            Rule clause = BuildClause(parameters[0]);
            if (clause == null)
            {
                // unbound variable or non callable argument
                return ListHelper.BuildList(Unification.Failed());
            }

            EnsureProgram(interpreter).AssertClause(clause, atEnd);
            return ListHelper.BuildList(Unification.Success());
        }

        private static LogSharpProgram EnsureProgram(Interpreter interpreter)
        {
            if (interpreter.LogSharpProgram == null)
            {
                interpreter.LogSharpProgram = new LogSharpProgram(new List<Rule>());
            }

            return interpreter.LogSharpProgram;
        }

        // Converts the argument of an assert into a clause : a term is a fact
        // (or a "Head :- Body" rule), an atom is a 0-arity fact.
        private static Rule BuildClause(Valuable argument)
        {
            switch (argument)
            {
                case Term term:
                {
                    if (term.Name == ImplyFunctor && term.Arity == 2)
                    {
                        Term head = HeadAsTerm(term.Parameters[0]);
                        if (head == null)
                        {
                            return null;
                        }

                        return new Predicate(head, FlattenConjunction(term.Parameters[1]));
                    }

                    return new Fact(term);
                }
                case Atom atom:
                {
                    return new Fact(atom.Name);
                }
                default:
                {
                    return null;
                }
            }
        }

        private static Term HeadAsTerm(Valuable head)
        {
            switch (head)
            {
                case Term term:
                {
                    return term;
                }
                case Atom atom:
                {
                    return new Term(atom.Name);
                }
                default:
                {
                    return null;
                }
            }
        }

        // A rule body given as a conjunction term (","/2 nested pairs) is
        // flattened into the flat list of goals the engine executes.
        private static List<Term> FlattenConjunction(Valuable body)
        {
            List<Term> goals = new List<Term>();
            FlattenConjunction(body, goals);
            return goals;
        }

        private static void FlattenConjunction(Valuable value, List<Term> goals)
        {
            if (value is Term term && term.Name == ConjunctionFunctor && term.Arity == 2)
            {
                FlattenConjunction(term.Parameters[0], goals);
                FlattenConjunction(term.Parameters[1], goals);
                return;
            }

            Term goal = HeadAsTerm(value);
            if (goal != null)
            {
                goals.Add(goal);
            }
            // non callable body elements are ignored
        }

        // ---------------------------------------------------------------------
        // retraction

        // Removes every clause matching the pattern. When exportBindings is set
        // (retract/1) one unification is produced per removed clause, binding
        // the pattern variables the way the matching did ; retractall/1 ignores
        // the bindings and simply removes. Returns null when the pattern is
        // invalid (unbound or non callable).
        private static List<Unification> Retract(Interpreter interpreter, Goal goal,
            List<Valuable> parameters, bool exportBindings)
        {
            if (parameters.Count != 1 || interpreter.LogSharpProgram == null)
            {
                return null;
            }

            // split "Head :- Body" patterns (rules are retracted by unifying
            // their head and their body) : plain terms match the clause head
            Valuable headPattern = parameters[0];
            Valuable bodyPattern = null;
            if (headPattern is Term pattern && pattern.Name == ImplyFunctor && pattern.Arity == 2)
            {
                headPattern = pattern.Parameters[0];
                bodyPattern = pattern.Parameters[1];
            }

            if (!(headPattern is Term headTerm))
            {
                // unbound or non callable pattern : nothing can be retracted
                return null;
            }

            // only the pattern's own variables are exported to the caller :
            // internal clause variables (renamed apart below) must not leak
            var patternVariables = new HashSet<string>();
            CollectVariableNames(headTerm, patternVariables);
            if (bodyPattern != null)
            {
                CollectVariableNames(bodyPattern, patternVariables);
            }

            string specification = headTerm.Specification;

            // snapshot the clause list : retracting while iterating (and while
            // choice points are active in this predicate) cannot corrupt the
            // enumeration
            List<Rule> candidates = interpreter.LogSharpProgram.GetRulesForSpec(specification).ToList();

            List<Unification> unifications = new List<Unification>();
            foreach (var clause in candidates)
            {
                // rename the clause variables apart so they cannot be captured
                // by (or capture) the pattern's variables
                Term freshHead = FreshenHead(clause.Head);
                Unification match = interpreter.Unifier.UnifyTerms(headTerm, goal.Unification,
                    freshHead, new Unification());
                if (match == null || !match.IsOk)
                {
                    continue;
                }

                if (bodyPattern != null)
                {
                    Unification bodyMatch = UnifyBody(interpreter, goal.Unification, bodyPattern, clause);
                    if (bodyMatch == null || !bodyMatch.IsOk)
                    {
                        continue;
                    }

                    match.AddBindings(bodyMatch);
                }

                // unlink the clause from every table (ordered lists, fallback
                // list, hash buckets) so it is never served again
                interpreter.LogSharpProgram.RetractClause(clause);

                if (exportBindings)
                {
                    unifications.Add(ExportPatternBindings(match, patternVariables));
                }
            }

            return unifications;
        }

        // unifies the body part of a "Head :- Body" retract pattern with the
        // body of the clause (true for facts, conjunction term for rules)
        private static Unification UnifyBody(Interpreter interpreter, Unification unification,
            Valuable bodyPattern, Rule clause)
        {
            Valuable bodyValue = BuildBodyValue(clause);

            if (bodyPattern is Variable variable)
            {
                Unification bound = Unification.Success();
                if (!variable.IsDiscardableVariable)
                {
                    bound.SetBinding(variable.Name, bodyValue);
                }

                return bound;
            }

            if (bodyPattern is Scalar patternScalar && bodyValue is Scalar valueScalar)
            {
                return patternScalar.Equals(valueScalar) ? Unification.Success() : Unification.Failed();
            }

            if (bodyPattern is Term patternTerm && bodyValue is Term valueTerm)
            {
                return interpreter.Unifier.UnifyTerms(patternTerm, unification,
                    FreshenHead(valueTerm), new Unification());
            }

            // e.g. the pattern body is "true" while the clause has a real body
            return Unification.Failed();
        }

        private static Valuable BuildBodyValue(Rule clause)
        {
            if (clause.Clauses == null || !clause.Clauses.Any())
            {
                return new Atom(TrueAtomName);
            }

            // fold the body goals back into a right-nested conjunction term
            Term conjunction = clause.Clauses[clause.Clauses.Count - 1];
            for (int i = clause.Clauses.Count - 2; i >= 0; i--)
            {
                conjunction = new Term(ConjunctionFunctor,
                    ListHelper.BuildList<Valuable>(clause.Clauses[i], conjunction));
            }

            return conjunction;
        }

        // keeps only the bindings of the pattern's variables : clause variables
        // taking part in the match must stay internal to the retraction
        private static Unification ExportPatternBindings(Unification match, HashSet<string> patternVariables)
        {
            Unification exported = Unification.Success();
            foreach (var binding in match.Environment.Environment)
            {
                if (patternVariables.Contains(binding.Key))
                {
                    exported.Environment[binding.Key] = binding.Value;
                }
            }

            return exported;
        }

        private static void CollectVariableNames(Valuable value, HashSet<string> names)
        {
            switch (value)
            {
                case Variable variable:
                {
                    if (!variable.IsDiscardableVariable)
                    {
                        names.Add(variable.Name);
                    }

                    break;
                }
                case Term term:
                {
                    if (term.Parameters != null)
                    {
                        foreach (var parameter in term.Parameters)
                        {
                            CollectVariableNames(parameter, names);
                        }
                    }

                    break;
                }
                // scalars and expressions hold no pattern variable
            }
        }

        // ---------------------------------------------------------------------
        // renaming apart

        private static int FreshenCounter;

        private static Term FreshenHead(Term term)
        {
            int invocationId = FreshenCounter++;
            return FreshenTerm(term, new Dictionary<string, Variable>(), invocationId);
        }

        private static Term FreshenTerm(Term term, Dictionary<string, Variable> freshVariables, int invocationId)
        {
            List<Valuable> parameters = new List<Valuable>();
            foreach (var parameter in term.Parameters)
            {
                parameters.Add(FreshenValue(parameter, freshVariables, invocationId));
            }

            return new Term(term.Name, parameters);
        }

        private static Valuable FreshenValue(Valuable value, Dictionary<string, Variable> freshVariables, int invocationId)
        {
            switch (value)
            {
                case Variable variable when variable.IsDiscardableVariable:
                {
                    return variable;
                }
                case Variable variable:
                {
                    if (!freshVariables.TryGetValue(variable.Name, out var freshVariable))
                    {
                        freshVariable = new Variable($"__retract{invocationId}_{variable.Name}");
                        freshVariables[variable.Name] = freshVariable;
                    }

                    return freshVariable;
                }
                case Term term:
                {
                    return FreshenTerm(term, freshVariables, invocationId);
                }
                default:
                {
                    return value;
                }
            }
        }

    }
}