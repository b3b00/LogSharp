
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using logSharp.helpers;

namespace logSharp.model
{
    public class Term : LogSharpAST, Valuable
    {
        public const string ListConstructor = "list";
        public bool IsScalar => false;

        public bool IsVariable => false;
        
        public bool IsDiscardableVariable => false;

        public bool IsTerm => true;
        
        public bool IsList => Name == ListConstructor && (Parameters.Count == 2 || Parameters.Count == 0);

        public bool IsConstantList => ListTail() == null || (!ListTail().IsVariable && ListTail().IsConstantList);

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


        public bool IsEmptyList => IsList && !Parameters.Any();
        
        public static Term EmptyListTerm()
        {
            return new Term() {Name = Term.ListConstructor};
        }

        public static Term ListTerm(Valuable head, Valuable tail)
        {
            var list = EmptyListTerm();
            list.Parameters = ListHelper.BuildList(head, tail);
            return list;
        }

        public static Term ListTerm(List<Valuable> parameters)
        {
            Term list = new Term();
            list.Name = Term.ListConstructor;
            
            Valuable emptyTail = EmptyListTerm();
            Valuable currentTail = emptyTail;
            
            
            for (int i = parameters.Count - 1; i >= 0; i--)
            {
                var currentParameter = parameters[i];
                if (currentTail == emptyTail && currentParameter.IsVariable)
                {
                    currentTail = currentParameter;
                }
                else
                {
                    Term term = new Term(Term.ListConstructor, new List<Valuable>() {currentParameter, currentTail});
                    currentTail = term;
                }
            }

            list.Parameters = (currentTail as Term)?.Parameters;
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


        public Valuable ListHead()
        {
            if (IsList)
            {
                if (Parameters.Any())
                {
                    return Parameters[0];
                }

                return null;
            }
            else
            {
                throw new InvalidOperationException($"Term {this.Dump()} is not a list.");
            }
        }

        public Term ListTail()
        {
            if (Parameters.Count >= 2)
            {
                return Parameters[1] as Term;
            }

            return null;
        }

        public List<Variable> GetFreeVariables()
        {
            List<Variable> freeVars = new List<Variable>();
            if (Parameters.Any())
            {
                freeVars = Parameters.SelectMany(x => x.GetFreeVariables()).ToList();
            }
            return freeVars;
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
                    if (IsConstantList)
                    {
                        var items = ToValuableList();
                        dump.Append(string.Join(", ", items.Select(i => i.Dump())));
                    }
                    else
                    {
                        dump.Append(Parameters[0].Dump());
                        dump.Append("|");
                        dump.Append(Parameters[1].Dump());
                    }
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