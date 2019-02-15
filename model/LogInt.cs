namespace logSharp.model
{
    public class LogInt : LogScalar
    {
        public int Value { get; set; }

        public LogInt(int value)
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
                if (o is LogInt other)
                {
                    return other.Value == Value;
                }
            }

            return false;
        }
    }
}