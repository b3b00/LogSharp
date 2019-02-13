
using System.Collections.Generic;

namespace logSharp.model
{
    public class LogClause : LogSharpAST
    {
        public string Name { get; set; }

        public List<LogScalar> Parameters { get; set; }

        public LogClause(string name, List<LogScalar> parameters)
        {
            Name = name;
            Parameters = parameters;
        }
    }
}