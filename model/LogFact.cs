using System.Collections.Generic;

namespace LogSharp.model
{
    public class LogFact : LogClause, LogRule
    {

        public LogFact(LogClause clause) : base(clause.Name, clause.Parameters)
        {
        }
        
        public LogFact(string name, List<LogScalar> parameters) : base(name, parameters)
        {
        }
    }
}