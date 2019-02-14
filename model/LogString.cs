namespace logSharp.model
{
    public class LogString : LogScalar
    {
        public string Value { get; set; }

        public LogString(string value)
        {
            Value = value;
        }
        
        public string Dump()
        {
            return $"\"{Value}\"";
        }
    }
}