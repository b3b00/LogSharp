namespace logSharp.model
{
    public class LogDouble : LogScalar
    {
        public double Value { get; set; }

        public LogDouble(double value)
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
                if (o is LogDouble other)
                {
                    return other.Value == Value;
                }
            }

            return false;
        }
    }
}