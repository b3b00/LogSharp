using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using logSharp.interpretation.unification;

namespace logSharp.model
{
    public class LogSharpProgram : LogSharpAST
    {
        public List<Rule> Rules { get; set; }
        
        public Dictionary<string, List<Rule>> RulesBySpec { get; set; }

        public Dictionary<string, PredicateIndex> PredicateIndexes { get; set; } = new();

        public bool UseIndex { get; set; } = true;

        public LogSharpProgram(List<Rule> rules)
        {
            Rules = rules;
            SetRulesBySpec();
        }

        private void SetRulesBySpec()
        {
            RulesBySpec = new Dictionary<string, List<Rule>>();
            PredicateIndexes = new Dictionary<string, PredicateIndex>();
            foreach (var rule in Rules)
            {
                AddRuleToIndices(rule);
            }
        }

        private void AddRuleToIndices(Rule rule)
        {
            if (!RulesBySpec.TryGetValue(rule.Specification, out var currentRules))
            {
                currentRules = new List<Rule>();
                RulesBySpec[rule.Specification] = currentRules;
            }
            currentRules.Add(rule);

            if (!PredicateIndexes.TryGetValue(rule.Specification, out var index))
            {
                index = new PredicateIndex();
                PredicateIndexes[rule.Specification] = index;
            }
            index.Add(rule);
        }

        public void Append(Rule rule)
        {
            AssertClause(rule, atEnd: true);
        }

        /// <summary>
        /// Adds a clause to the program at runtime (assertz when <paramref
        /// name="atEnd"/>, asserta otherwise). The clause is registered in the
        /// ordered rule lists AND in the first argument index, which is kept
        /// synchronized : the head's first argument is categorized (variable,
        /// constant, compound) and appended/prepended to the matching bucket
        /// and to the fallback list. Unknown predicates are created on the fly.
        /// </summary>
        public void AssertClause(Rule rule, bool atEnd = true)
        {
            if (rule == null) return;
            Rules ??= new List<Rule>();
            RulesBySpec ??= new Dictionary<string, List<Rule>>();
            PredicateIndexes ??= new Dictionary<string, PredicateIndex>();

            if (!RulesBySpec.TryGetValue(rule.Specification, out var currentRules))
            {
                currentRules = new List<Rule>();
                RulesBySpec[rule.Specification] = currentRules;
            }

            if (!PredicateIndexes.TryGetValue(rule.Specification, out var index))
            {
                index = new PredicateIndex();
                PredicateIndexes[rule.Specification] = index;
            }

            if (atEnd)
            {
                Rules.Add(rule);
                currentRules.Add(rule);
                index.Add(rule);
            }
            else
            {
                // asserta : the clause must be evaluated FIRST, the logical
                // ordering of every structure is preserved
                Rules.Insert(0, rule);
                currentRules.Insert(0, rule);
                index.AddFirst(rule);
            }
        }

        /// <summary>
        /// Removes a clause from the ordered rule lists and from the first
        /// argument index (every table it was classified into). Returns true
        /// when the clause was present in the program.
        /// </summary>
        public bool RetractClause(Rule rule)
        {
            if (rule == null) return false;

            bool removed = false;
            if (Rules != null)
            {
                removed = Rules.Remove(rule);
            }

            if (RulesBySpec != null && RulesBySpec.TryGetValue(rule.Specification, out var currentRules))
            {
                removed |= currentRules.Remove(rule);
            }

            if (PredicateIndexes != null && PredicateIndexes.TryGetValue(rule.Specification, out var index))
            {
                removed |= index.Remove(rule);
            }

            return removed;
        }

        public void Append(LogSharpProgram program)
        {
            if (program != null && program.Rules != null && program.Rules.Any())
            {
                foreach (var rule in program.Rules)
                {
                    Append(rule);
                }
            }
        }

        public List<Rule> GetRulesForSpec(string specification)
        {
            if (RulesBySpec.ContainsKey(specification))
            {
                return RulesBySpec[specification];
            }
            return new List<Rule>();
        }

        public PredicateIndex GetPredicateIndex(string specification)
        {
            if (PredicateIndexes != null && PredicateIndexes.TryGetValue(specification, out var index))
            {
                return index;
            }
            return null;
        }

        public IEnumerable<Rule> GetCandidateRules(string specification, Valuable goalFirstArg = null, Unification unification = null)
        {
            if (UseIndex)
            {
                var index = GetPredicateIndex(specification);
                if (index != null)
                {
                    // the index returns a snapshot, isolated from the structural
                    // mutations assert/retract may perform while the enumeration
                    // is active (choice points over the modified predicate)
                    return index.GetCandidateClauses(goalFirstArg, unification);
                }
            }

            // copy the ordered clause list so dynamic updates cannot mutate the
            // list being iterated by an active search
            return new List<Rule>(GetRulesForSpec(specification));
        }
        
        [ExcludeFromCodeCoverage]
        public string Dump()
        {
            return $"\"{Rules.Select(r => r.Dump()).Aggregate((string r1, string r2) => r1+"\n"+r2)}\"";
        }
    }
}
