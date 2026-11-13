using System.Diagnostics.CodeAnalysis;
using logSharp.interpretation.unification;

namespace logSharp.model
{
    public class Atom : Scalar, IExpression
    {
        public new string Name { get; private set; }

        public override object RawValue => Name;

        public object Evaluate(Unification unification) => RawValue;
        
        public Atom(string name)
        {
            Name = name;
        }

        [ExcludeFromCodeCoverage]
        public override string Dump()
        {
            return Name;
        }

        [ExcludeFromCodeCoverage]
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