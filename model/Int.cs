namespace logSharp.model
{
    public class Int : Scalar
    {
        public int Value { get; set; }

        public Int(int value)
        {
            Value = value;
        }
        
        public override string Dump()
        {
            return Value.ToString();
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
    }
}