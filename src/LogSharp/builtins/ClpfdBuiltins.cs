using System;
using System.Collections.Generic;
using System.Linq;
using logSharp.helpers;
using logSharp.interpretation;
using logSharp.interpretation.unification;
using logSharp.model;

namespace logSharp.builtins
{
    // minimal finite-domain constraint store shared by the clpfd-like builtins
    // (all_distinct/1, ins_range/3, label/1). Constraints are recorded when a goal is
    // called with still-unbound variables, and checked by label/1 while it
    // enumerates the assignments.
    public class ClpfdConstraints
    {
        // a group of variables (and already-bound values) that must be pairwise distinct
        public class DistinctGroup
        {
            public List<string> Variables { get; } = new List<string>();

            public List<int> FixedValues { get; } = new List<int>();
        }

        public List<DistinctGroup> DistinctGroups { get; } = new List<DistinctGroup>();

        // inclusive domain bounds per variable name
        public Dictionary<string, Tuple<int, int>> Domains { get; } =
            new Dictionary<string, Tuple<int, int>>();
    }

    [BuiltIns]
    public class ClpfdBuiltins
    {
        // default domain used by label/1 for variables without an ins_range/3 declaration
        private const int DefaultDomainMin = 1;

        private const int DefaultDomainMax = 9;

        // internal name of the range-constraint built-in; the infix sugar
        // "Vars ins Min...Max" is translated by the parser into a Term with
        // exactly this name
        public const string InsBuiltinName = "ins_range";
        // all_distinct/1 : every element of the list must be pairwise different.
        // Bound elements are checked immediately ; free variables are recorded as a
        // distinct-group constraint enforced later by label/1.
        [BuiltIn("all_distinct/1")]
        public static List<Unification> AllDistinct(Interpreter interpreter, Goal goal,
            List<Valuable> parameters)
        {
            Valuable first = ListBuiltins.ResolveParameter(parameters[0], goal.Unification);
            if (!(first is Term list && list.IsList))
            {
                return ListHelper.BuildList(Unification.Failed());
            }

            var group = new ClpfdConstraints.DistinctGroup();
            foreach (var item in list.ToValuableList())
            {
                if (IsDiscardable(item))
                {
                    continue;
                }

                var chased = ChaseCell(item, goal.Unification);
                if (chased.Item1 != null)
                {
                    if (group.Variables.Contains(chased.Item1))
                    {
                        // the same variable appears twice : it cannot be distinct from itself
                        return ListHelper.BuildList(Unification.Failed());
                    }

                    group.Variables.Add(chased.Item1);
                }
                else if (chased.Item2.HasValue)
                {
                    if (group.FixedValues.Contains(chased.Item2.Value))
                    {
                        return ListHelper.BuildList(Unification.Failed());
                    }

                    group.FixedValues.Add(chased.Item2.Value);
                }
                else
                {
                    // unsupported element kind
                    return ListHelper.BuildList(Unification.Failed());
                }
            }

            if (group.Variables.Count >= 2)
            {
                interpreter.Constraints.DistinctGroups.Add(group);
            }

            return ListHelper.BuildList(Unification.Success());
        }

        // ins_range/3 : ins_range(Variables, Min, Max) constrains every variable of
        // the list to the inclusive integer range [Min, Max]. Already bound elements
        // must lie within the range.
        // Written in programs as the infix sugar "Vars ins Min...Max".
        // The bounds may be integer literals, variables bound to an integral value
        // or arithmetic expressions evaluating to an integral value.
        [BuiltIn($"{InsBuiltinName}/3")]
        public static List<Unification> Ins(Interpreter interpreter, Goal goal,
            List<Valuable> parameters)
        {
            Valuable list = ListBuiltins.ResolveParameter(parameters[0], goal.Unification);
            int? min = ResolveBound(parameters[1], goal.Unification);
            int? max = ResolveBound(parameters[2], goal.Unification);

            if (!(list is Term listTerm && listTerm.IsList)
                || min == null
                || max == null
                || min.Value > max.Value)
            {
                return ListHelper.BuildList(Unification.Failed());
            }

            foreach (var item in listTerm.ToValuableList())
            {
                if (IsDiscardable(item))
                {
                    continue;
                }

                var chased = ChaseCell(item, goal.Unification);
                if (chased.Item1 != null)
                {
                    interpreter.Constraints.Domains[chased.Item1] = Tuple.Create(min.Value, max.Value);
                }
                else if (chased.Item2.HasValue)
                {
                    if (chased.Item2.Value < min.Value || chased.Item2.Value > max.Value)
                    {
                        return ListHelper.BuildList(Unification.Failed());
                    }
                }
                else
                {
                    return ListHelper.BuildList(Unification.Failed());
                }
            }

            return ListHelper.BuildList(Unification.Success());
        }

        // resolves a range bound : the bound is an IExpression (integer literal,
        // variable or arithmetic expression) evaluated against the current
        // unification, and must produce an integral value (2.5 is rejected, 3.0
        // is accepted as 3). Returns null when the bound cannot be resolved.
        private static int? ResolveBound(Valuable bound, Unification unification)
        {
            var resolved = ListBuiltins.ResolveParameter(bound, unification);
            object result = resolved;
            if (resolved is IExpression expression && !(resolved is Int))
            {
                try
                {
                    result = expression.Evaluate(unification);
                }
                catch (Exception)
                {
                    return null;
                }
            }

            switch (result)
            {
                case Int intValue:
                    return intValue.Value;
                case int intValue:
                    return intValue;
                case double doubleValue when doubleValue == Math.Floor(doubleValue):
                    return (int) doubleValue;
                default:
                    return null;
            }
        }

        // label/1 : assigns a value from the variable domain to every free variable
        // of the list, backtracking on the recorded constraints (distinct groups and
        // domains). Each complete consistent assignment is returned as one
        // unification, so the surrounding query can enumerate every solution.
        [BuiltIn("label/1")]
        public static List<Unification> Label(Interpreter interpreter, Goal goal,
            List<Valuable> parameters)
        {
            Valuable first = ListBuiltins.ResolveParameter(parameters[0], goal.Unification);
            if (!(first is Term list && list.IsList))
            {
                return ListHelper.BuildList(Unification.Failed());
            }

            // ordered list of the free variables to label (de-duplicated)
            var order = new List<string>();
            foreach (var item in list.ToValuableList())
            {
                if (IsDiscardable(item))
                {
                    continue;
                }

                var chased = ChaseCell(item, goal.Unification);
                if (chased.Item1 != null && !order.Contains(chased.Item1))
                {
                    order.Add(chased.Item1);
                }
            }

            var solutions = new List<Dictionary<string, int>>();
            if (order.Count > 0)
            {
                LabelSearch(interpreter, goal.Unification, order, solutions);
            }

            var results = new List<Unification>();
            if (order.Count == 0)
            {
                // nothing free to label : every element was already bound, and the
                // recorded constraints (all_distinct/ins) were checked at call time
                results.Add(Unification.Success());
            }

            foreach (var solution in solutions)
            {
                var unification = new Unification();
                foreach (var pair in solution)
                {
                    unification.SetBinding(pair.Key, new Int(pair.Value));
                }

                results.Add(unification);
            }

            return results;
        }

        private static void LabelSearch(Interpreter interpreter, Unification unification,
            List<string> order, List<Dictionary<string, int>> solutions)
        {
            // groups involving at least one labeled variable, indexed by variable
            var groupsOf = new Dictionary<string, List<ClpfdConstraints.DistinctGroup>>();
            foreach (var group in interpreter.Constraints.DistinctGroups)
            {
                foreach (var variable in group.Variables)
                {
                    if (order.Contains(variable))
                    {
                        if (!groupsOf.TryGetValue(variable, out var groups))
                        {
                            groups = new List<ClpfdConstraints.DistinctGroup>();
                            groupsOf[variable] = groups;
                        }

                        groups.Add(group);
                    }
                }
            }

            var assignments = new Dictionary<string, int>();

            bool Consistent(string name, int value)
            {
                if (!groupsOf.TryGetValue(name, out var groups))
                {
                    return true;
                }

                foreach (var group in groups)
                {
                    foreach (var fixedValue in group.FixedValues)
                    {
                        if (fixedValue == value)
                        {
                            return false;
                        }
                    }

                    foreach (var other in group.Variables)
                    {
                        if (other == name)
                        {
                            continue;
                        }

                        if (assignments.TryGetValue(other, out var assigned))
                        {
                            if (assigned == value)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            // the variable may have been bound outside of this labeling
                            var bound = BoundValue(other, unification);
                            if (bound.HasValue && bound.Value == value)
                            {
                                return false;
                            }
                        }
                    }
                }

                return true;
            }

            void Solve(int index)
            {
                if (index == order.Count)
                {
                    solutions.Add(new Dictionary<string, int>(assignments));
                    return;
                }

                var name = order[index];
                Tuple<int, int> domain = interpreter.Constraints.Domains.TryGetValue(name, out var declared)
                    ? declared
                    : Tuple.Create(DefaultDomainMin, DefaultDomainMax);

                for (int value = domain.Item1; value <= domain.Item2; value++)
                {
                    assignments[name] = value;
                    if (Consistent(name, value))
                    {
                        Solve(index + 1);
                    }
                }

                assignments.Remove(name);
            }

            Solve(0);
        }

        // transpose/2 : transpose(Matrix, Transposed). Works in both directions and
        // checks a fully bound pair. The transposed lists share the very same cell
        // variables as the original ones, so constraints recorded on one side apply
        // to the other.
        [BuiltIn("transpose/2")]
        public static List<Unification> Transpose(Interpreter interpreter, Goal goal,
            List<Valuable> parameters)
        {
            Valuable first = ListBuiltins.ResolveParameter(parameters[0], goal.Unification);
            Valuable second = ListBuiltins.ResolveParameter(parameters[1], goal.Unification);

            if (first is Term rows && rows.IsList)
            {
                Term columns = BuildTranspose(rows, goal.Unification);
                if (columns == null)
                {
                    return ListHelper.BuildList(Unification.Failed());
                }

                if (second is Variable outVariable && !outVariable.IsDiscardableVariable)
                {
                    var unification = new Unification();
                    unification.SetBinding(outVariable.Name, columns);
                    return ListHelper.BuildList(unification);
                }

                if (second is Term expected && expected.IsList)
                {
                    var unified = interpreter.UnifyTerms(columns, new Unification(), expected,
                        new Unification());
                    return ListHelper.BuildList(unified);
                }

                return ListHelper.BuildList(Unification.Failed());
            }

            if (first is Variable inVariable && !inVariable.IsDiscardableVariable
                && second is Term transposed && transposed.IsList)
            {
                // transposing the transposed matrix gives back the original one
                Term backRows = BuildTranspose(transposed, goal.Unification);
                if (backRows == null)
                {
                    return ListHelper.BuildList(Unification.Failed());
                }

                var unification = new Unification();
                unification.SetBinding(inVariable.Name, backRows);
                return ListHelper.BuildList(unification);
            }

            return ListHelper.BuildList(Unification.Failed());
        }

        // maplist/2 : maplist(Goal, List) applies the unary goal to every element.
        // The goal may be a partially applied term, e.g. maplist(same_length(Rows), Rows).
        [BuiltIn("maplist/2")]
        public static List<Unification> MapList(Interpreter interpreter, Goal goal,
            List<Valuable> parameters)
        {
            Valuable goalValue = ListBuiltins.ResolveParameter(parameters[0], goal.Unification);
            Valuable listValue = ListBuiltins.ResolveParameter(parameters[1], goal.Unification);

            if (!(listValue is Term list && list.IsList))
            {
                return ListHelper.BuildList(Unification.Failed());
            }

            Term goalTemplate;
            if (goalValue is Term goalTerm)
            {
                goalTemplate = goalTerm;
            }
            else if (goalValue is Atom atom)
            {
                goalTemplate = new Term(atom.Name);
            }
            else
            {
                return ListHelper.BuildList(Unification.Failed());
            }

            var accumulated = new Unification();
            if (goal.Unification != null)
            {
                accumulated.AddBindings(goal.Unification);
            }

            foreach (var item in list.ToValuableList())
            {
                var callParameters = new List<Valuable>(goalTemplate.Parameters) {item};
                var callTerm = new Term(goalTemplate.Name, callParameters);
                var question = new Question(callTerm);
                var ok = interpreter.Search(question, accumulated)
                    .FirstOrDefault(result => result != null && result.IsOk);
                if (ok == null)
                {
                    return ListHelper.BuildList(Unification.Failed());
                }

                var merged = new Unification();
                merged.AddBindings(accumulated);
                merged.AddBindings(ok);
                accumulated = merged;
            }

            return ListHelper.BuildList(accumulated);
        }

        // chases a list element through variable alias bindings.
        // Returns (variableName, null) for a still free variable,
        // or (null, value) for a variable/integer bound to an Int,
        // or (null, null) when the element cannot be handled.
        private static Tuple<string, int?> ChaseCell(Valuable item, Unification unification)
        {
            if (item is Int intValue)
            {
                return Tuple.Create((string) null, (int?) intValue.Value);
            }

            if (!(item is Variable))
            {
                return Tuple.Create((string) null, (int?) null);
            }

            var name = ((Variable) item).Name;
            while (unification != null && unification.HasBinding(name))
            {
                var bound = unification.GetBinding(name);
                if (bound is Int boundInt)
                {
                    return Tuple.Create((string) null, (int?) boundInt.Value);
                }

                if (bound is Variable aliased)
                {
                    name = aliased.Name;
                    continue;
                }

                return Tuple.Create((string) null, (int?) null);
            }

            return Tuple.Create(name, (int?) null);
        }

        // value of a variable already bound (through aliases) to an Int, null otherwise
        private static int? BoundValue(string name, Unification unification)
        {
            if (unification == null)
            {
                return null;
            }

            var current = name;
            while (unification.HasBinding(current))
            {
                var bound = unification.GetBinding(current);
                if (bound is Int boundInt)
                {
                    return boundInt.Value;
                }

                if (bound is Variable aliased)
                {
                    current = aliased.Name;
                    continue;
                }

                return null;
            }

            return null;
        }

        private static bool IsDiscardable(Valuable item)
        {
            return item is Variable variable && variable.IsDiscardableVariable;
        }

        // builds the transposed list of lists ; the cell values (variables or ints)
        // are shared with the input matrix. Returns null when the input is not a
        // rectangular list of lists.
        private static Term BuildTranspose(Term matrix, Unification unification)
        {
            var resolvedRows = new List<Term>();
            foreach (var row in matrix.ToValuableList())
            {
                var current = row;
                while (current is Variable variable && unification != null
                       && unification.HasBinding(variable.Name))
                {
                    current = unification.GetBinding(variable.Name);
                }

                if (!(current is Term rowTerm && rowTerm.IsList))
                {
                    return null;
                }

                resolvedRows.Add(rowTerm);
            }

            if (!resolvedRows.Any())
            {
                return Term.ListTerm(new List<Valuable>());
            }

            int width = resolvedRows[0].ToValuableList().Count;
            var columns = new List<Valuable>();
            for (int index = 0; index < width; index++)
            {
                var column = new List<Valuable>();
                foreach (var row in resolvedRows)
                {
                    var items = row.ToValuableList();
                    if (items.Count != width)
                    {
                        return null;
                    }

                    column.Add(items[index]);
                }

                columns.Add(Term.ListTerm(column));
            }

            return Term.ListTerm(columns);
        }
    }
}
