using logSharp.interpretation.unification;

namespace logSharp.builtins.expressions.model
{
    public sealed class ExpressionVariable : Expression
    {
        private readonly string VariableName;

        public ExpressionVariable(string varName)
        {
            VariableName = varName;
        }


        public object Evaluate(Unification unification)
        {
            return null;
        }
    }
}