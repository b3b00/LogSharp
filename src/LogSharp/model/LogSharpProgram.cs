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
            Rules.Add(rule);
            AddRuleToIndices(rule);
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
                    return index.GetCandidateClauses(goalFirstArg, unification);
                }
            }
            return GetRulesForSpec(specification);
        }
        
        [ExcludeFromCodeCoverage]
        public string Dump()
        {
            return $"\"{Rules.Select(r => r.Dump()).Aggregate((string r1, string r2) => r1+"\n"+r2)}\"";
        }
    }
}
