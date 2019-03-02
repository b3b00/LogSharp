namespace logSharp.model
{
    public class String : Scalar
    {
        public String(string value)
        {
            Value = value;
        }

        public string Value { get; set; }

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
                if (o is String other)
                    return other.Value == Value;

            return false;
        }


        public override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : 0;
        }
    }
}