namespace logSharp.model
{
    public class LogVariable : LogValuable
    {
        public bool IsScalar => false;

        public bool IsVariable => true;

        public bool IsTerm => false;
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