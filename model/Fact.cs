using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace logSharp.model
{
    public class Fact : Rule
    {

        
        
        public Fact(Term clause) : base(clause, null)
        {
        }

        public Fact(string name, List<Valuable> parameters) : base(new Term(name,parameters))
        {
        }

        public Fact(string name) : base(new Term(name))
        {
        }

        public override string Dump()
        {
            StringBuilder dump = new StringBuilder();
            dump.Append(Name);

            dump.Append("(");
            if (Parameters.Any())
            {

                bool first = true;
                foreach (var parameter in Parameters)
                {
                    if (!first)
                    {
                        dump.Append(", ");
                    }

                    dump.Append(parameter.Dump());
                    first = false;
                }


            }

            dump.Append(").");
            return dump.ToString();
        }
    }

}
