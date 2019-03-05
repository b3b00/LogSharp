namespace logSharp.model
{
    public class LogString : Scalar
    {
        
        public override object RawValue => Value;
        public string Value { get; set; }

        public LogString(string value)
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
                if (o is LogString other)
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