
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;

namespace logSharp.model
{
    public class Term : LogSharpAST, Valuable
    {
        public const string listConstructor = "list";
        public bool IsScalar => false;

        public bool IsVariable => false;

        public bool IsTerm => true;
        
        public bool IsList => Name == listConstructor && (Parameters.Count == 2 || Parameters.Count == 0);

        public string Name { get; set; }

        public List<Valuable> Parameters { get; set; }
        
        public int Arity => Parameters.Count;
        public string Specification => $"{Name}/{Arity}";

        public Term(string name) : this (name, new List<Valuable>())
        {
            ;
        }

        public Term() : this(null, new List<Valuable>())
        {
        }
        

        public Term(string name, List<Valuable> parameters)
        {
            Name = name;
            Parameters = parameters;
        }

        public virtual string Dump()
        {
            var dump =  new StringBuilder();

            if (!IsList)
            {
                dump.Append(Name);

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
            }
            else
            {
                dump.Append("[");
                if (Parameters.Any())
                {
                    dump.Append(Parameters[0].Dump());
                    dump.Append("|");
                    dump.Append(Parameters[1].Dump());
                }
                dump.Append("]");
            }


            return dump.ToString();
        }

        public override string ToString()
        {
            return Dump();
        }

        public bool Equals(object o)
        {
            if (o != null)
            {
                if (o is Term other)
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