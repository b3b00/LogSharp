using System.Diagnostics.CodeAnalysis;
using logSharp.interpretation.unification;

namespace logSharp.model
{
    public class Int : Scalar, IExpression
    {
        public int Value { get; set; }
        
        public override object RawValue => Value;

        public Int(int value)
        {
            Value = value;
        }
        
        [ExcludeFromCodeCoverage]
        public override string Dump()
        {
            return Value.ToString();
        }

        public object Evaluate(Unification unification) => RawValue;

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return Dump();
        }


        public override bool Equals(object o)
        {
            if (o != null)
            {
                if (o is Int other)
                {
                    return other.Value == Value;
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Value;
        }
    }
}