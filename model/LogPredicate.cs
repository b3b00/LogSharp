using System.Collections.Generic;
using System.Text;

namespace logSharp.model
{
    public class LogPredicate : LogRule
    {
        public LogClause Head { get; set; }
        
        public List<LogClause> Clauses { get; set; }

        public LogPredicate(LogClause head, List<LogClause> clauses)
        {
            Head = head;
            Clauses = clauses;
        }
        
        public string Dump()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(Head.Dump());
            builder.Append(" :- ");
            bool isFirst = true;
            foreach (var clause in Clauses)
            {
                if (!isFirst)
                {
                    builder.Append(", ");
                }

                builder.Append(clause.Dump());
                isFirst = false;
            }

            builder.Append(".");

            return builder.ToString();
        }
    }
}