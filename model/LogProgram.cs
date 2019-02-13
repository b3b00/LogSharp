using System.Collections.Generic;

namespace LogSharp.model
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