using System.Collections.Generic;
namespace LogSharp.model
{
    public class LogPredicate : LogRule
    {
        public LogClause Head { get; set; }
        
        public List<LogClause> Clauses { get; set; }

        public LogPredicate(LogClause head, List<LogClause> clauses)
        {
            Head = Head;
            Clauses = Clauses;
        }
    }
}