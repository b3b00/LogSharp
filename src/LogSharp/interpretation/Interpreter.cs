using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeepCopy;
using logSharp.builtins;
using logSharp.interpretation.unification;
using logSharp.model;
using logSharp.parser;
using LogSharp.prelude;
using sly.parser;

namespace logSharp.interpretation
{

    public delegate List<Unification> PrologBuiltIn(Interpreter interpreter, Goal goal,
        List<Valuable> parameters);

    public class Interpreter
    {



        public bool Verbose { get; set; }

        public string DebugOutput { get; set; }

        public LogSharpProgram LogSharpProgram { get; set; }

        public IUnifier Unifier { get; set; }

        public Parser<LogSharpToken, LogSharpAST> Parser { get; private set; }

        public Dictionary<string, PrologBuiltIn> BuiltIns { get; set; }

        // finite-domain constraint store shared with the clpfd-like builtins
        public ClpfdConstraints Constraints { get; } = new ClpfdConstraints();


        public Interpreter(bool verbose, LogSharpProgram logSharpProgram, Parser<LogSharpToken, LogSharpAST> parser,
            IUnifier unifier)
        {
            LogSharpProgram = logSharpProgram;
            Verbose = verbose;
            Parser = parser;
            Unifier = unifier;
            BuiltIns = new Dictionary<string, PrologBuiltIn>();
            BuiltinsLoader.LoadBuiltIns(this);
            PreludeLoader.LoadPrelude(this);
        }

        public void AddBuiltIn(string spec, PrologBuiltIn builtIn)
        {
            if (!BuiltIns.ContainsKey(spec))
            {
                BuiltIns[spec] = builtIn;
            }
        }

        private bool TryGetBuiltIn(string spec, out PrologBuiltIn builtIn)
        {
            return BuiltIns.TryGetValue(spec, out builtIn);
        }

        public void Append(Rule rule)
        {
            if (LogSharpProgram == null)
            {

                LogSharpProgram = new LogSharpProgram(new List<Rule>());
            }

            LogSharpProgram.Append(rule);
        }

        public void Append(LogSharpProgram program)
        {
            if (LogSharpProgram == null)
            {
                LogSharpProgram = program;
            }
            else
            {
                LogSharpProgram.Append(program);
            }
        }

        public bool HasRule(string name)
        {
            return LogSharpProgram.RulesBySpec.ContainsKey(name);
        }

        public bool HasBuiltin(string name)
        {
            return BuiltIns.ContainsKey(name);
        }



        public void Trace(string message)
        {
            if (Verbose)
            {
                Console.WriteLine(message);
                if (DebugOutput != null)
                {
                    File.AppendAllText(DebugOutput, message + "\n");
                }
            }
        }

        public void LoadProgramFile(string program)
        {
            string prog = "";
            if (File.Exists(program))
                prog = File.ReadAllText(program);
            else if (File.Exists(program + ".pl"))
                prog = File.ReadAllText(program + ".pl");
            else if (File.Exists(program + ".pro"))
                prog = File.ReadAllText(program + ".pro");
            LoadProgram(prog);
        }

        public void LoadProgram(string program)
        {

            var logprog = Parser.Parse(program, "program");
            if (logprog.IsOk)
            {
                Append(logprog.Result as LogSharpProgram);
            }
            else
            {
                Console.WriteLine("errors");
                foreach (var error in logprog.Errors)
                {
                    Console.WriteLine($"\t{error.ErrorMessage}");
                }
            }
        }

        /// <summary>
        /// Eagerly computes and returns every solution of <paramref name="question"/>.
        /// Kept for backward compatibility; builtins and tests that need the whole set
        /// of solutions use this. Interactive callers should prefer
        /// <see cref="SearchLazy"/> so the search is not fully evaluated up front.
        /// </summary>
        public IList<Unification> Search(Question question, Unification unification = null)
        {
            return SearchLazy(question, unification).ToList();
        }

        /// <summary>
        /// Enumerates the solutions of <paramref name="question"/> lazily: the search
        /// is suspended between two solutions, so the next one is only computed when
        /// the caller asks for it (via MoveNext). Callers that need every solution can
        /// simply materialize the sequence with ToList().
        /// </summary>
        public IEnumerable<Unification> SearchLazy(Question question, Unification unification = null)
        {
            Trace("*****************************************");
            Trace("***");
            Trace($"*** {question}");
            Trace("***");
            Trace("*****************************************");
            bool resultFound = false;
            Stack<Goal> stack = new Stack<Goal>();
            Trace($"search question {question}");
            if (unification != null && unification.IsOk)
            {
                Term evaled = Unifier.EvalTerm(question, unification);
                question = new Question(evaled);
            }

            Goal rootGoal = new Goal(question);
            rootGoal.Goals.Add(rootGoal);
            Trace($"stck : {rootGoal.ToString()}");
            stack.Push(rootGoal);

            while (stack.Any())
            {
                var stackAsList = stack.ToList();
                Goal current = stack.Pop();
                if (current.IsCut)
                {
                    continue;
                }
                Trace("");
                Trace("____________________________");
                Trace("____________________________");
                Trace("");
                Trace($"pop {current}");

                if (current.Index >= current.Count || current.IsFact)
                {
                    if (current.IsRootGoal)
                    {
                        if (current.Unification.IsOk)
                        {
                            resultFound = true;
                            yield return current.Unification;
                        }

                        continue;
                    }

                    var p = DeepCopier.Copy(current.Parent);
                    Unification u = new Unification();
                    if (current.IsFact)
                    {
                        u = Unifier.UnifyTerms(current.Head, current.Unification,
                            p.Goals[p.Index].Head, p.Unification);
                    }
                    else if (current.Index >= current.Count)
                    {
                        var destination = current.IsContinuation
                            ? p.Head
                            : p.Goals[p.Index].Head;
                        u = Unifier.UnifyTerms(current.Head, current.Unification,
                            destination, p.Unification);
                    }
                    else
                    {
                        ;
                    }

                    if (u.IsOk)
                    {
                        if (current.IsContinuation)
                        {
                            // A continuation goal has already executed ALL the remaining
                            // goals of its parent (from Index+1 to Count). So the parent
                            // must be re-pushed past those goals (at Count), NOT at
                            // Index+1 which would re-execute them.
                            // Also, the parent must inherit the whole continuation
                            // unification, not only the head-parameter bindings returned
                            // by UnifyTerms, otherwise body-local bindings (like the
                            // list built by bag_of/3) are lost for the next conjuncts.
                            var merged = new Unification();
                            merged.AddBindings(p.Unification);
                            merged.AddBindings(current.Unification);
                            merged.AddBindings(u);
                            p.Unification = merged;
                            p.Index = current.Count;
                        }
                        else
                        {
                            p.Index++;
                            p.Unification = u;
                        }

                        Trace($"stck {p} @ {p.Index}");
                        stack.Push(p);
                    }

                    continue;
                }

                var parent = DeepCopier.Copy(current.Parent);
                var term = current.Goals[current.Index];


                if (TryGetBuiltIn(term.Specification, out var builtIn))
                {
                    var builtInTerm = DeepCopier.Copy(term.Head);
                    builtInTerm = Unifier.EvalTerm(builtInTerm, current.Unification);
                    Trace($"calling built-in {term.Specification} {term.ToString()} with {current.Unification}");
                    var builtInUnifications = builtIn(this, current, builtInTerm.Parameters);
                    foreach (var builtInUnification in builtInUnifications)
                    {
                        Trace($"        >>> {builtInUnification.ToString()}");
                    }

                    if (current.Rule != null)
                    {
                        if (builtInUnifications != null && builtInUnifications.Any())
                        {
                            foreach (var builtInUnification in builtInUnifications)
                            {
                                if (builtInUnification.IsOk)
                                {
                                    Goal built = new Goal(current.Rule, current, builtInUnification, false);
                                    built.Unification.AddBindings(current.Unification);

                                    built.Index = current.Index + 1;
                                    Trace($"stck {built} @ {built.Index}");
                                    stack.Push(built);
                                }
                            }
                        }
                    }
                    else
                    {
                        resultFound = true;
                        foreach (var builtInUnification in builtInUnifications)
                        {
                            yield return builtInUnification;
                        }
                    }
                }
                else
                {
                    List<Rule> applicableRules = LogSharpProgram.GetRulesForSpec(term.Specification);
                    foreach (var applicable in (applicableRules))
                    {
                        Trace($"trying rule {applicable.ToString()}");
                        Goal child = new Goal(applicable, current, current.Unification);

                        var unif = Unifier.UnifyTerms(term.Head, current.Unification, child.Head,
                            child.Unification);
                        if (unif.IsOk)
                        {
                            child.Unification = unif;
                            Trace($"stck {child} @ {child.Index}");
                            stack.Push(child);
                            current.AddChild(child);
                        }
                        else
                        {
                            Trace("unification failed.");
                        }
    
                        
                    }
                }

            }

            if (!resultFound)
            {
                yield return Unification.Failed();
            }
        }

        public Unification UnifyTerms(Term source, Unification sourceEnv, Term destination,
            Unification destEnv)
            => Unifier.UnifyTerms(source, sourceEnv, destination, destEnv);


        
        public void Cut(Goal goal)
        {
            var siblings = goal.Parent?.Parent?.Children;
            if (siblings != null && siblings.Any())
            {
                for (int i = goal.Index; i < siblings.Count; i++)
                {
                    siblings[i].Cut();
                }
            }
        }
    }
}