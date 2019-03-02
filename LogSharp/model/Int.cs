namespace logSharp.model
{
    public class Int : Scalar
    {
        public Int(int value)
        {
            Value = value;
        }

        public int Value { get; set; }

        public override string Dump()
        {
            return Value.ToString();
        }

        public override string ToString()
        {
            return Dump();
        }


        public override bool Equals(object o)
        {
            if (o != null)
                if (o is Int other)
                    return other.Value == Value;

            return false;
        }

        public override int GetHashCode()
        {
            return Value;
        }
    }
}