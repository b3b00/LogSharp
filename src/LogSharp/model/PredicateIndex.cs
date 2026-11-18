using System.Collections.Generic;
using System.Linq;
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
            Classify(clause, atEnd: true);
        }

        /// <summary>
        /// Prepends a clause (asserta) : it must be evaluated FIRST by subsequent
        /// calls, so the logical ordering is preserved. The hash bucket and the
        /// fallback list are both updated to keep the index synchronized.
        /// </summary>
        public void AddFirst(Clause clause)
        {
            if (clause == null) return;
            _allClauses.Insert(0, clause);
            Classify(clause, atEnd: false);
        }

        /// <summary>
        /// Removes a clause from the fallback list and from the hash bucket it
        /// was classified into, so the index never serves a retracted clause
        /// again. Returns true when the clause was found somewhere.
        /// </summary>
        public bool Remove(Clause clause)
        {
            if (clause == null) return false;
            bool removed = _allClauses.Remove(clause);

            if (clause.Parameters == null || clause.Parameters.Count == 0)
            {
                return removed;
            }

            var firstArg = clause.Parameters[0];

            if (firstArg == null || firstArg is Variable || firstArg.IsVariable)
            {
                removed |= _variableClauses.Remove(clause);
            }
            else if (firstArg is Scalar constant)
            {
                object key = constant.RawValue;
                if (_constantTable.TryGetValue(key, out var bucket))
                {
                    removed |= bucket.Remove(clause);
                    RemoveBucketIfEmpty(_constantTable, key, bucket);
                }
            }
            else if (firstArg is Term compound)
            {
                var key = new FunctorArity(compound.Functor, compound.Arity);
                if (_compoundTable.TryGetValue(key, out var bucket))
                {
                    removed |= bucket.Remove(clause);
                    RemoveBucketIfEmpty(_compoundTable, key, bucket);
                }
            }
            else
            {
                removed |= _variableClauses.Remove(clause);
            }

            return removed;
        }

        // emptied buckets are unregistered so the tables never keep stale keys
        private static void RemoveBucketIfEmpty<TKey>(Dictionary<TKey, List<Clause>> table,
            TKey key, List<Clause> bucket)
        {
            if (!bucket.Any())
            {
                table.Remove(key);
            }
        }

        // Append or prepend the clause to the bucket matching the category of
        // its first argument (variable / constant / compound term).
        private void Classify(Clause clause, bool atEnd)
        {
            if (clause.Parameters == null || clause.Parameters.Count == 0)
            {
                return;
            }

            var firstArg = clause.Parameters[0];

            if (firstArg == null || firstArg is Variable || firstArg.IsVariable)
            {
                AddToBucket(_variableClauses, clause, atEnd);
            }
            else if (firstArg is Scalar constant)
            {
                object key = constant.RawValue;
                if (!_constantTable.TryGetValue(key, out var bucket))
                {
                    bucket = new List<Clause>();
                    _constantTable[key] = bucket;
                }
                AddToBucket(bucket, clause, atEnd);
            }
            else if (firstArg is Term compound)
            {
                var key = new FunctorArity(compound.Functor, compound.Arity);
                if (!_compoundTable.TryGetValue(key, out var bucket))
                {
                    bucket = new List<Clause>();
                    _compoundTable[key] = bucket;
                }
                AddToBucket(bucket, clause, atEnd);
            }
            else
            {
                AddToBucket(_variableClauses, clause, atEnd);
            }
        }

        private static void AddToBucket(List<Clause> bucket, Clause clause, bool atEnd)
        {
            if (atEnd)
            {
                bucket.Add(clause);
            }
            else
            {
                bucket.Insert(0, clause);
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

        /// <summary>
        /// Returns the candidate clauses for a goal, in logical order, as a
        /// SNAPSHOT. The returned list is a copy isolated from the store : a
        /// clause asserted or retracted while the caller iterates the result
        /// (choice points being active in the middle of a predicate being
        /// modified) can never corrupt the enumeration or crash it.
        /// </summary>
        public List<Clause> GetCandidateClauses(Valuable goalFirstArg, Unification unification = null)
        {
            if (goalFirstArg == null)
            {
                return new List<Clause>(_allClauses);
            }

            // 1. Dereference the term to get its current binding state
            Valuable derefArg = Dereference(goalFirstArg, unification);

            if (derefArg == null || derefArg is Variable || derefArg.IsVariable)
            {
                // If the first argument is uninstantiated, ALL clauses might match
                return new List<Clause>(_allClauses);
            }

            List<Clause> candidates = new List<Clause>();

            // 2. Check constant index if it's a primitive
            if (derefArg is Scalar constant)
            {
                if (_constantTable.TryGetValue(constant.RawValue, out var bucket))
                {
                    candidates.AddRange(bucket);
                }
            }
            // 3. Check compound index if it's a structured term
            else if (derefArg is Term compound)
            {
                var key = new FunctorArity(compound.Functor, compound.Arity);
                if (_compoundTable.TryGetValue(key, out var bucket))
                {
                    candidates.AddRange(bucket);
                }
            }
            else
            {
                return new List<Clause>(_allClauses);
            }

            // 4. Always include variable-headed clauses as fallback
            candidates.AddRange(_variableClauses);
            return candidates;
        }

        public List<Clause> GetCandidateClauses(Term goalFirstArg)
            => GetCandidateClauses((Valuable)goalFirstArg, null);
    }
}
