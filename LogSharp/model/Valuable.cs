namespace logSharp.model
{
    public interface Valuable : LogSharpAST
    {
        bool IsScalar { get; }
        bool IsVariable { get; }

        bool IsDiscardableVariable { get; }

        bool IsTerm { get; }

        bool IsList { get; }

        bool IsConstantList { get; }
        string Name { get; }
    }
}