namespace logSharp.model
{
    public class Atom : Scalar
    {
        public string Name { get; set; }

        public Atom(string name)
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
                if (o is Atom other)
                {
                    return other.Name == Name;
                }
            }

            return false;
        }
    }
}