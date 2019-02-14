using System.Collections.Generic;

namespace logSharp.model
{
    public class LogFact : LogClause, LogRule
    {

        public LogFact(LogClause clause) : base(clause.Name, clause.Parameters)
        {
        }
        
        public LogFact(string name, List<LogScalar> parameters) : base(name, parameters)
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