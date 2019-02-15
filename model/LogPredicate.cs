using System.Collections.Generic;
using System.Text;

namespace logSharp.model
{
    public class LogPredicate : LogRule
    {
        public LogTerm Head { get; set; }
        
        public List<LogTerm> Clauses { get; set; }

        public LogPredicate(LogTerm head, List<LogTerm> clauses)
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