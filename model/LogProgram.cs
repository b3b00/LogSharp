using System.Collections.Generic;

namespace logSharp.model
{
    public class LogProgram : LogSharpAST
    {
        public List<LogRule> Rules { get; set; }

        public LogProgram(List<LogRule> rules)
        {
            Rules = rules;
        }
    }
}