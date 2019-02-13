namespace logSharp.model
{
    public class LogDouble : LogScalar
    {
        public double Value { get; set; }

        public LogDouble(double value)
        {
            Value = value;
        }
    }
}