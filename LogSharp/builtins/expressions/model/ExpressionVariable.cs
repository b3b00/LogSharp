using logSharp.interpretation.unification;
using logSharp.model;

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
            if (unification.Environment.ContainsVariable(VariableName))
            {
                var logValue = unification.Environment[VariableName];
                if (logValue is Scalar scalar)
                {
                    return scalar.RawValue;
                }
            }

            return null;
        }
    }
}