namespace logSharp.model
{
    public class Atom : Scalar
    {
        public new string Name { get; private set; }

        public override object RawValue => Name;
        
        public Atom(string name)
        {
            Name = name;
        }

        public override string Dump()
        {
            return Name;
        }

        public override string ToString()
        {
            return Dump();
        }

        public override bool Equals(object o)
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

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}