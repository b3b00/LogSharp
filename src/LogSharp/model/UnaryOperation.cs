using System.Diagnostics.CodeAnalysis;
using logSharp.interpretation.unification;
using logSharp.parser;

namespace logSharp.model
{
    public class UnaryOperation : Scalar, LogSharpAST, IExpression
    {
        private readonly LogSharpToken _operator;
        
        public LogSharpToken Operator => _operator;
        
        private readonly LogSharpAST _expression;
        
        public LogSharpAST Expression => _expression;

        public UnaryOperation(LogSharpToken op, LogSharpAST right)
        {
            _operator = op;
            _expression = right;
        }

        public object Evaluate(Unification unification)
        {
            var right = ((IExpression)_expression).Evaluate(unification);

            if (right != null && right is int i)
            {
                switch (_operator)
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

        [ExcludeFromCodeCoverage]
        public override string Dump()
        {
            string oper = _operator == LogSharpToken.Minus ? "-" : "+";
            return $"{oper} {_expression.Dump()}";
        }
    }
}