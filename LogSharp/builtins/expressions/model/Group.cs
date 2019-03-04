using logSharp.interpretation.unification;

namespace logSharp.builtins.expressions.model
{
    public class Group : Expression
    {
        private readonly Expression InnerExpression;

        public Group(Expression expr)
        {
            InnerExpression = expr;
        }

        public object Evaluate(Unification unification)
        {
            return InnerExpression.Evaluate(unification);
        }
    }
}