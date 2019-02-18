namespace logSharp.model
{
    public abstract class Scalar : LogSharpAST, Valuable
    {
        public bool IsScalar => true;
        public bool IsVariable => false;
        public bool IsTerm => false;
        
        public bool IsList => false;
        public string Name => null;

        public bool IsVAriable => false;
        public abstract string Dump();
    }
}