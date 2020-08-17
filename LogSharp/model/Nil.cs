namespace logSharp.model
{
    public class Nil : Term
    {

        public new string Name => "()";

        private static Nil _instance;

        public static Nil Instance { 
            get {
            if (_instance == null)
            {
                _instance = new Nil();
            }

            return _instance;
            }
            set { }
        }

    protected Nil()
        {
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
            return  o != null && o is Atom;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}