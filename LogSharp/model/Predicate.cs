using System.Collections.Generic;
using System.Text;

namespace logSharp.model
{
    public class Predicate : Rule
    {
        public Predicate(Term head, List<Term> clauses) : base(head, clauses)
        {
        }


        public override string ToString()
        {
            return Dump();
        }

        public override string Dump()
        {
            var builder = new StringBuilder();
            builder.Append(Head.Dump());
            builder.Append(" :- ");
            var isFirst = true;
            foreach (var clause in Clauses)
            {
                if (!isFirst) builder.Append(", ");

                builder.Append(clause.Dump());
                isFirst = false;
            }

            builder.Append(".");

            return builder.ToString();
        }
    }
}