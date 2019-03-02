using System;

namespace logSharp.model
{
    public class Double : Scalar
    {
        public Double(double value)
        {
            Value = value;
        }

        public double Value { get; set; }

        public override string Dump()
        {
            return Value.ToString();
        }

        public override string ToString()
        {
            return Dump();
        }

        public override bool Equals(object o)
        {
            if (o != null)
                if (o is Double other)
                    return Math.Abs(other.Value - Value) < 0.000001;

            return false;
        }


        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}