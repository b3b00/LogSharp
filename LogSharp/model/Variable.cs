namespace logSharp.model
{
    public class Variable : Valuable
    {
        public const string DiscardingVariable = "_";

        public Variable(string name)
        {
            Name = name;
        }

        public bool IsScalar => false;

        public bool IsVariable => true;

        public bool IsDiscardableVariable => Name == DiscardingVariable;

        public bool IsTerm => false;

        public bool IsList => false;

        public bool IsConstantList => true;
        public string Name { get; set; }

        public string Dump()
        {
            return Name;
        }

        public override string ToString()
        {
            return Dump();
        }
    }
}