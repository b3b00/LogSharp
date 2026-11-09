using System.Collections.Generic;

namespace logSharp.model
{
    public abstract class Scalar : LogSharpAST, Valuable
    {
        
        public virtual object RawValue { get; }
        public bool IsScalar => true;
        public bool IsVariable => false;
        
        public bool IsDiscardableVariable => false;
        public bool IsTerm => false;
        
        public bool IsList => false;
        public string Name => null;

        public bool IsConstantList => true;

        public bool IsVAriable => false;
        public abstract string Dump();
        
        public virtual List<Variable> GetFreeVariables() => new List<Variable>(); 
    }
}