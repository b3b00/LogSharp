using System.Collections.Generic;
using System.Linq;

namespace logSharp.model
{
    public class LogSharpProgram : LogSharpAST
    {
        public List<Rule> Rules { get; set; }
        
        public Dictionary<string,List<Rule>> RulesBySpec { get; set; }

        public LogSharpProgram(List<Rule> rules)
        {
            Rules = rules;
            SetRulesBySpec();
        }

        private void SetRulesBySpec()
        {
            RulesBySpec = Rules.GroupBy(r => r.Specification).ToDictionary(g =>  g.Key, g => g.ToList());
        }

        public void Append(Rule rule)
        {
            Rules.Add(rule);
            SetRulesBySpec();
        }

        public void Append(LogSharpProgram program)
        {
            if (program != null && program.Rules != null && program.Rules.Any())
            {
                Rules = program.Rules;
                SetRulesBySpec();
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
        
        
        
        public string Dump()
        {
            return $"\"{Rules.Select(r => r.Dump()).Aggregate((string r1, string r2) => r1+"\n"+r2)}\"";
        }
        
    }
}