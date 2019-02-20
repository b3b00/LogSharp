
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;

namespace logSharp.model
{
    public class Term : LogSharpAST, Valuable
    {
        public const string ListConstructor = "list";
        public bool IsScalar => false;

        public bool IsVariable => false;

        public bool IsTerm => true;
        
        public bool IsList => Name == ListConstructor && (Parameters.Count == 2 || Parameters.Count == 0);

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
        
        #region LISTS


        public static Term EmptyListTerm()
        {
            return new Term() {Name = Term.ListConstructor};
        }

        public static Term ListTerm(List<Valuable> parameters)
        {
            Term list = new Term();
            list.Name = Term.ListConstructor;
            
            Valuable tail = EmptyListTerm();
            
            
            for (int i = parameters.Count - 1; i >= 0; i--)
            {
                Term term = new Term(Term.ListConstructor,new List<Valuable>(){parameters[i],tail});
                tail = term;
            }

            list.Parameters = (tail as Term).Parameters;
            return list;
        }

        public List<Valuable> ToValuableList()
        {
            if (IsList)
            {
                if (Parameters.Any())
                {
                    var list = new List<Valuable>();

                    if (Parameters[0] is Valuable headValue)
                    {
                        list.Add(headValue);
                    }

                    if (Parameters.Count == 2)
                    {
                        if (Parameters[1] is Term tailList && tailList.IsList)
                        {
                            list.AddRange(tailList.ToValuableList());
                        }
                        else if (Parameters[1] is Valuable tailValue)
                        {
                            list.Add(tailValue);
                        }
                    }

                    return list;
                }
                return new List<Valuable>();
            }
            else
            {
                throw new InvalidOperationException($"Term {this.Dump()} is not a list.");
            }
        }

        
        
        #endregion

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

        public override bool Equals(object o)
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

       

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ (Parameters != null ? Parameters.GetHashCode() : 0);
            }
        }
    }
}