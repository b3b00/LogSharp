using System.Collections.Generic;

namespace logSharp.model
{
    public class LogFact : LogTerm, LogRule
    {

        public LogFact(LogTerm clause) : base(clause.Name, clause.Parameters)
        {
        }
        
        public LogFact(string name, List<LogValuable> parameters) : base(name, parameters)
        {
        }
        
        public LogFact(string name) : base(name)
        {
        }

        public override  string Dump()
        {
            string clause = base.Dump();
            return $"{clause}.";
        }
        
    }
}