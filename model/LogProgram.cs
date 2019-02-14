using System.Collections.Generic;
using System.Linq;

namespace logSharp.model
{
    public class LogProgram : LogSharpAST
    {
        public List<LogRule> Rules { get; set; }

        public LogProgram(List<LogRule> rules)
        {
            Rules = rules;
        }
        
        public string Dump()
        {
            return $"\"{Rules.Select(r => r.Dump()).Aggregate((string r1, string r2) => r1+"\n"+r2)}\"";
        }
        
    }
}