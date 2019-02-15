namespace logSharp.model
{
    public class LogAtom : LogScalar
    {
        public string Name { get; set; }

        public LogAtom(string name)
        {
            Name = name;
        }

        public override string Dump()
        {
            return Name;
        }
        
        public bool Equals(object o)
        {
            if (o != null)
            {
                if (o is LogAtom other)
                {
                    return other.Name == Name;
                }
            }

            return false;
        }
    }
}