using logSharp.interpretation.unification;

namespace logSharp.builtins.expressions.model
{
    public class UnaryOperation : Expression
    {
        private readonly ExpressionToken Operator;
        private readonly Expression RightExpression;

        public UnaryOperation(ExpressionToken op, Expression right)
        {
            Operator = op;
            RightExpression = right;
        }

        public object Evaluate(Unification unification)
        {
            var right = RightExpression.Evaluate(unification);

            if (right != null && right is int i)
            {
                switch (Operator)
                {
                    case ExpressionToken.PLUS:
                    {
                        return i;
                    }
                    case ExpressionToken.MINUS:
                    {
                        return i;
                    }
                    default:
                    {
                        return null;
                    }
                }
            }

            return null;
        }
    }
}