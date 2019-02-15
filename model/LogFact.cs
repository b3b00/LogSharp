using System.Collections.Generic;

namespace logSharp.model
{
    public class LogFact : LogTerm, LogRule
    {

        public LogFact(LogTerm clause) : base(clause.Name, clause.Parameters)
        {
        }
        
        public LogFact(string name, List<LogSharpAST> parameters) : base(name, parameters)
        {
        }
        
        public LogFact(string name) : base(name)
        {
        }

        public new string Dump()
        {
            string clause = base.Dump();
            return $"{clause}.";
        }
        
    }
}