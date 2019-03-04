using logSharp.interpretation.unification;

namespace logSharp.builtins.expressions.model
{
    public sealed class Number : Expression
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
    }
}