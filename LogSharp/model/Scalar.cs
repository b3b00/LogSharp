using System.Collections.Generic;
using logSharp.interpretation.unification;

namespace logSharp.model
{
    public abstract class Scalar :  IValuable,  IExpression
    {
        public virtual object RawValue { get; }
        
        public bool IsScalar => true;
        public bool IsVariable => false;
        
        public bool IsDiscardableVariable => false;
        public bool IsTerm => false;
        
        public bool IsList => false;
        public string Name => null;

        public bool IsConstantList => false;

        public Dictionary<string,object> Data { get; set; }

        public abstract string Dump();

        public virtual object Evaluate(Unification unification)
        {
            return RawValue;
        }
    }
}