
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;

namespace logSharp.model
{
    public class LogTerm : LogSharpAST, LogValuable
    {
        public bool IsScalar => false;

        public bool IsVariable => false;

        public bool IsTerm => true;
        public string Name { get; set; }

        public List<LogValuable> Parameters { get; set; }

        public LogTerm(string name) : this (name, new List<LogValuable>())
        {
            ;
        }

        public LogTerm() : this(null, new List<LogValuable>())
        {
        }
        

        public LogTerm(string name, List<LogValuable> parameters)
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
        
        public bool Equals(object o)
        {
            if (o != null)
            {
                if (o is LogTerm other)
                {
                    if (other.Name == Name)
                    {
                        if (Parameters.Count == other.Parameters.Count)
                        {
                            for (int i = 0; i < Parameters.Count; i++)
                            {
                                var p1 = Parameters[i];
                                var p2 = other.Parameters[i];
                                if (p1 != p2)
                                {
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}