namespace LogSharp.model
{
    public class LogVariable : LogScalar
    {
        public string Name { get; set; }

        public LogVariable(string name)
        {
            Name = name;
        }
        
    }
}