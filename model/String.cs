namespace logSharp.model
{
    public class String : Scalar
    {
        public string Value { get; set; }

        public String(string value)
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
                if (o is String other)
                {
                    return other.Value == Value;
                }
            }

            return false;
        }
    }
}