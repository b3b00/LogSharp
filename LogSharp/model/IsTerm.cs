
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using logSharp.helpers;

namespace logSharp.model
{
    public class IsTerm : Term
    {
        
        public IExpression Expression { get; set; }
        
        public string Specification => $"is/2";

        public int Arity => 2;

        public Variable Variable { get; set; }

        public IsTerm(string name, Variable variable, IExpression expression) : base (name,ListHelper.BuildList<IValuable>(variable,expression))
        {
            Variable = variable;
            Expression = expression;
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

        public virtual string Dump() => $"is( {Variable}, {Expression})";

        public override string ToString()
        {
            return Dump();
        }

        public override bool Equals(object o)
        {
            if (o != null)
            {
                if (o is IsTerm other)
                {
                    return Variable.Equals(other.Variable) && Expression.Equals(other.Expression);
                }
            }

            return false;
        }

       

        public override int GetHashCode()
        {
            return 0;
        }
    }
}