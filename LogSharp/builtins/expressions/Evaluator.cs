using logSharp.builtins.expressions.model;
using logSharp.interpretation.unification;

namespace logSharp.builtins.expressions
{
    public class Evaluator
    {
        public static object Evaluate(Expression expression, Unification unification)
        {
            return null;

            switch (expression)
            {
                case Group group:
                {
                    break;
                }
                case BinaryOperation binary:
                {
                    break;
                }
                case UnaryOperation unary:
                {
                    break;
                }
                case Number number:
                {
                    break;
                }
                case ExpressionVariable variable:
                {
                    break;
                }
            }
        }

    }
}