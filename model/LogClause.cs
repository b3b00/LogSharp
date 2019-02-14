
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace logSharp.model
{
    public class LogClause : LogSharpAST
    {
        public string Name { get; set; }

        public List<LogScalar> Parameters { get; set; }

        public LogClause(string name) : this (name, new List<LogScalar>())
        {
            ;
        }

        public LogClause() : this(null, new List<LogScalar>())
        {
        }
        

        public LogClause(string name, List<LogScalar> parameters)
        {
            Name = name;
            Parameters = parameters;
        }

        public virtual string Dump()
        {
            var dump =  new StringBuilder(Name);
            
            if (Parameters.Any())
            {
                dump.Append("(");
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
                dump.Append(")");
            }
    
            
            return dump.ToString();
        }
    }
}