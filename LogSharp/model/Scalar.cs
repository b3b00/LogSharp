using System.Collections.Generic;

namespace logSharp.model
{
    public abstract class Scalar : LogSharpAST, Valuable
    {
        public bool IsScalar => true;
        public bool IsVariable => false;
        
        public bool IsDiscardableVariable => false;
        public bool IsTerm => false;
        
        public bool IsList => false;
        public string Name => null;

        public bool IsConstantList => true;

        public bool IsVAriable => false;

        public Dictionary<string,object> Data { get; set; }

        public abstract string Dump();
    }
}