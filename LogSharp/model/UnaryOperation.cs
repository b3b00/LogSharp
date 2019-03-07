using logSharp.interpretation.unification;
using logSharp.parser;

namespace logSharp.model
{
    public class UnaryOperation : Scalar, ILogSharpAst, IExpression
    {
        private readonly LogSharpToken Operator;
        private readonly ILogSharpAst Expression;

        public UnaryOperation(LogSharpToken op, ILogSharpAst right)
        {
            Operator = op;
            Expression = right;
        }

        public override object Evaluate(Unification unification)
        {
            var right = ((IExpression)Expression).Evaluate(unification);

            if (right != null && right is int i)
            {
                switch (Operator)
                {
                    case LogSharpToken.Plus:
                    {
                        return i;
                    }
                    case LogSharpToken.Minus:
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

        public override string Dump()
        {
            string oper = Operator == LogSharpToken.Minus ? "-" : "+";
            return $"{oper} {Expression.Dump()}";
        }
    }
}