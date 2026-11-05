using logSharp.interpretation.unification;

namespace logSharp.model
{
    public sealed class Number : LogSharpAST
    {
        private readonly int Value;

        public Number(int value)
        {
            Value = value;
        }

        public object Evaluate(Unification unification)
        {
            return Value;
        }

        public string Dump()
        {
            throw new System.NotImplementedException();
        }
    }
}