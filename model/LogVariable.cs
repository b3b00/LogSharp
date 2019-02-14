namespace logSharp.model
{
    public class LogVariable : LogScalar
    {
        public string Name { get; set; }

        public LogVariable(string name)
        {
            Name = name;
        }
        
        public string Dump()
        {
            return Name;
        }
        
    }
}