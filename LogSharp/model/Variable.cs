using System.Collections.Generic;
using logSharp.interpretation.unification;

namespace logSharp.model
{
    public class Variable : IValuable, IExpression
    {

        public const string DiscardingVariable = "_";
        
        public bool IsScalar => false;

        public bool IsVariable => true;
        
        public bool IsDiscardableVariable => Name == DiscardingVariable;

        public bool IsTerm => false;
        
        public bool IsList => false;
        
        public bool IsConstantList => true;
        public string Name { get; set; }
        
        public Dictionary<string,object> Data { get; set; }

        public Variable(string name)
        {
            Name = name;
        }
        
        public string Dump()
        {
            return Name;
        }

        public override string ToString()
        {
            return Dump();
        }
        
        public object Evaluate(Unification unification)
        {
            if (unification.Environment.ContainsVariable(Name))
            {
                var logValue = unification.Environment[Name];
                if (logValue is Scalar scalar)
                {
                    return scalar.RawValue;
                }
            }

            return null;
        }
    }
}