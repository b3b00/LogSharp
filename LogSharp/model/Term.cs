
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using logSharp.helpers;

namespace logSharp.model
{
    public class Term : IValuable
    {
        public const string ListConstructor = "list";
        public bool IsScalar => false;

        public bool IsVariable => false;
        
        public bool IsDiscardableVariable => false;

        public bool IsTerm => true;
        
        public bool IsList => Name == ListConstructor && (Parameters.Count == 2 || Parameters.Count == 0);

        public bool IsConstantList => ListTail() == null || (!ListTail().IsVariable && ListTail().IsConstantList);

        public string Name { get; set; }
        
        public Dictionary<string,object> Data { get; set; }

        public List<IValuable> Parameters { get; set; }
        
        public int Arity => Parameters.Count;
        public string Specification => $"{Name}/{Arity}";

        public Term(string name) : this (name, new List<IValuable>())
        {
            ;
        }

        public Term() : this(null, new List<IValuable>())
        {
        }
        

        public Term(string name, List<IValuable> parameters)
        {
            Name = name;
            Parameters = parameters;
            Data = new Dictionary<string, object>();
        }
        
        #region LISTS


        public bool IsEmptyList => IsList && !Parameters.Any();
        
        public static Term EmptyListTerm()
        {
            return new Term() {Name = Term.ListConstructor};
        }

        public static Term ListTerm(IValuable head, IValuable tail)
        {
            var list = EmptyListTerm();
            list.Parameters = ListHelper.BuildList(head, tail);
            return list;
        }

        public static Term ListTerm(List<IValuable> parameters)
        {
            Term list = new Term();
            list.Name = Term.ListConstructor;
            
            IValuable emptyTail = EmptyListTerm();
            IValuable currentTail = emptyTail;
            
            
            for (int i = parameters.Count - 1; i >= 0; i--)
            {
                var currentParameter = parameters[i];
                if (currentTail == emptyTail && currentParameter.IsVariable)
                {
                    currentTail = currentParameter;
                }
                else
                {
                    Term term = new Term(Term.ListConstructor, new List<IValuable>() {currentParameter, currentTail});
                    currentTail = term;
                }
            }

            list.Parameters = (currentTail as Term)?.Parameters;
            return list;
        }

        public List<IValuable> ToValuableList()
        {
            if (IsList)
            {
                if (Parameters.Any())
                {
                    var list = new List<IValuable>();

                    if (Parameters[0] is IValuable headValue)
                    {
                        list.Add(headValue);
                    }

                    if (Parameters.Count == 2)
                    {
                        if (Parameters[1] is Term tailList && tailList.IsList)
                        {
                            list.AddRange(tailList.ToValuableList());
                        }
                        else if (Parameters[1] is IValuable tailValue)
                        {
                            list.Add(tailValue);
                        }
                    }

                    return list;
                }
                return new List<IValuable>();
            }
            else
            {
                throw new InvalidOperationException($"Term {this.Dump()} is not a list.");
            }
        }


        public IValuable ListHead()
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