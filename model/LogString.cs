namespace logSharp.model
{
    public class LogString : LogScalar
    {
        public string Value { get; set; }

        public LogString(string value)
        {
            Value = value;
        }
        
        public override string Dump()
        {
            return $"\"{Value}\"";
        }
        
        public bool Equals(object o)
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
    }
}