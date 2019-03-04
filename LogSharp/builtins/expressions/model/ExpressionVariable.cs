namespace logSharp.builtins.expressions.model
{
    public sealed class ExpressionVariable : Expression
    {
        private readonly string VariableName;

        public ExpressionVariable(string varName)
        {
            VariableName = varName;
        }


        public int? Evaluate(ExpressionContext context)
        {
            return context.GetValue(VariableName);
        }
    }
}