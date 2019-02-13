namespace LogSharp.model
{
    public class LogInt : LogScalar
    {
        public int Value { get; set; }

        public LogInt(int value)
        {
            Value = value;
        }
    }
}