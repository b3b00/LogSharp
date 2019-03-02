namespace logSharp.model
{
    public class Atom : Scalar
    {
        public Atom(string name)
        {
            Name = name;
        }

        public new string Name { get; }

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
                if (o is Atom other)
                    return other.Name == Name;

            return false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}