namespace logSharp.model
{
    public class LogInt : Scalar
    {
        
        public override object RawValue => Value;
        
        public int Value { get; set; }

        public LogInt(int value)
        {
            Value = value;
        }
        
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
                if (o is LogInt other)
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