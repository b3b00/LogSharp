namespace logSharp.model
{
    public class Double : Scalar
    {
        public double Value { get; set; }

        public Double(double value)
        {
            Value = value;
        }

        public override string Dump()
        {
            return Value.ToString();
        }
        
        public bool Equals(object o)
        {
            if (o != null)
            {
                if (o is Double other)
                {
                    return other.Value == Value;
                }
            }

            return false;
        }
    }
}