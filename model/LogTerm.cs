
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace logSharp.model
{
    public class LogTerm : LogSharpAST
    {
        public string Name { get; set; }

        public List<LogSharpAST> Parameters { get; set; }

        public LogTerm(string name) : this (name, new List<LogSharpAST>())
        {
            ;
        }

        public LogTerm() : this(null, new List<LogSharpAST>())
        {
        }
        

        public LogTerm(string name, List<LogSharpAST> parameters)
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