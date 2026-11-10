using logSharp.interpretation.unification;

namespace logSharp.model
{
    public class String : Scalar, IExpression
    {
        
        public override object RawValue => Value;
        public string Value { get; set; }

        public String(string value)
        {
            Value = value;
        }
        
        public object Evaluate(Unification unification) => RawValue;
        
        public override string Dump()
        {
            return $"\"{Value}\"";
        }

        public override string ToString()
        {
            return Dump();
        }

        public override bool Equals(object o)
        {
            if (o != null)
            {
                if (o is String other)
                {
                    return other.Value == Value;
                }
            }

            return false;
        }

       

        public override int GetHashCode()
        {
            return (Value != null ? Value.GetHashCode() : 0);
        }
    }
}