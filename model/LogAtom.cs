namespace LogSharp.model
{
    public class LogAtom : LogScalar
    {
        public string Name { get; set; }

        public LogAtom(string name)
        {
            Name = name;
        }
        
    }
}