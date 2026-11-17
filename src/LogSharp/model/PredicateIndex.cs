using System.Collections.Generic;
using logSharp.interpretation.unification;

namespace logSharp.model
{
    using Clause = Rule;

    public class PredicateIndex
    {
        // Clauses whose first argument is a variable (must always be tried)
        internal readonly List<Clause> _variableClauses = new();

        // Exact match for atoms, numbers, and primitive constants
        internal readonly Dictionary<object, List<Clause>> _constantTable = new();

        // Match for compound terms based on Functor/Arity (e.g., "."/2)
        internal readonly Dictionary<FunctorArity, List<Clause>> _compoundTable = new();

        // Fallback ordered list for clauses where indexing isn't applicable
        internal readonly List<Clause> _allClauses = new();

        public IReadOnlyList<Clause> VariableClauses => _variableClauses;
        public IReadOnlyDictionary<object, List<Clause>> ConstantTable => _constantTable;
        public IReadOnlyDictionary<FunctorArity, List<Clause>> CompoundTable => _compoundTable;
        public IReadOnlyList<Clause> AllClauses => _allClauses;

        public void Add(Clause clause)
        {
            if (clause == null) return;
            _allClauses.Add(clause);

            if (clause.Parameters == null || clause.Parameters.Count == 0)
            {
                return;
            }

            var firstArg = clause.Parameters[0];

            if (firstArg == null || firstArg is Variable || firstArg.IsVariable)
            {
                _variableClauses.Add(clause);
            }
            else if (firstArg is Scalar constant)
            {
                object key = constant.RawValue;
                if (!_constantTable.TryGetValue(key, out var bucket))
                {
                    bucket = new List<Clause>();
                    _constantTable[key] = bucket;
                }
                bucket.Add(clause);
            }
            else if (firstArg is Term compound)
            {
                var key = new FunctorArity(compound.Functor, compound.Arity);
                if (!_compoundTable.TryGetValue(key, out var bucket))
                {
                    bucket = new List<Clause>();
                    _compoundTable[key] = bucket;
                }
                bucket.Add(clause);
            }
            else
            {
                _variableClauses.Add(clause);
            }
        }

        public Valuable Dereference(Valuable goalFirstArg, Unification unification = null)
        {
            if (goalFirstArg == null) return null;
            Valuable current = goalFirstArg;
            if (unification != null)
            {
                int depth = 0;
                while (current is Variable v && unification.HasBinding(v.Name) && depth < 100)
                {
                    var next = unification.GetBinding(v.Name);
                    if (next == null || ReferenceEquals(next, current)) break;
                    current = next;
                    depth++;
                }
            }
            return current;
        }

        public Valuable Dereference(Valuable goalFirstArg) => Dereference(goalFirstArg, null);

        public IEnumerable<Clause> GetCandidateClauses(Valuable goalFirstArg, Unification unification = null)
        {
            if (goalFirstArg == null)
            {
                foreach (var c in _allClauses) yield return c;
                yield break;
            }

            // 1. Dereference the term to get its current binding state
            Valuable derefArg = Dereference(goalFirstArg, unification);

            if (derefArg == null || derefArg is Variable || derefArg.IsVariable)
            {
                // If the first argument is uninstantiated, ALL clauses might match
                foreach (var c in _allClauses) yield return c;
                yield break;
            }

            List<Clause> bucket = null;

            // 2. Check constant index if it's a primitive
            if (derefArg is Scalar constant)
            {
                if (_constantTable.TryGetValue(constant.RawValue, out bucket))
                {
                    foreach (var c in bucket) yield return c;
                }
            }
            // 3. Check compound index if it's a structured term
            else if (derefArg is Term compound)
            {
                var key = new FunctorArity(compound.Functor, compound.Arity);
                if (_compoundTable.TryGetValue(key, out bucket))
                {
                    foreach (var c in bucket) yield return c;
                }
            }
            else
            {
                foreach (var c in _allClauses) yield return c;
                yield break;
            }

            // 4. Always include variable-headed clauses as fallback
            foreach (var c in _variableClauses)
            {
                yield return c;
            }
        }

        public IEnumerable<Clause> GetCandidateClauses(Term goalFirstArg)
            => GetCandidateClauses((Valuable)goalFirstArg, null);
    }
}
