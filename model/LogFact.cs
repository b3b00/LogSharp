using System.Collections.Generic;
using System.Linq;
using System.Text;

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
