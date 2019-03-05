using logSharp.builtins.expressions.model;
using sly.lexer;
using sly.parser.generator;

namespace logSharp.builtins.expressions
{
    public class VariableExpressionParser

    {
        [Operation((int) ExpressionToken.PLUS, Affix.InFix, Associativity.Right, 10)]
        [Operation((int) ExpressionToken.MINUS, Affix.InFix, Associativity.Left, 10)]
        public Expression BinaryTermExpression(Expression left, Token<ExpressionToken> operation, Expression right)
        {
            return new BinaryOperation(left, operation.TokenID, right);
        }


        [Operation((int) ExpressionToken.TIMES, Affix.InFix, Associativity.Right, 50)]
        [Operation((int) ExpressionToken.DIVIDE, Affix.InFix, Associativity.Left, 50)]
        public Expression BinaryFactorExpression(Expression left, Token<ExpressionToken> operation, Expression right)
        {
            return new BinaryOperation(left, operation.TokenID, right);
        }


        [Operation((int) ExpressionToken.MINUS, Affix.PreFix, Associativity.Right, 100)]
        public Expression PreFixExpression(Token<ExpressionToken> operation, Expression value)
        {
            return new UnaryOperation(operation.TokenID, value);
        }

        
        [Operation((int) ExpressionToken.EQUALS, Affix.InFix, Associativity.Right, 5)]
        [Operation((int) ExpressionToken.DIFFERENT, Affix.InFix, Associativity.Left, 5)]
        [Operation((int) ExpressionToken.GTE, Affix.InFix, Associativity.Left, 5)]
        [Operation((int) ExpressionToken.LTE, Affix.InFix, Associativity.Left, 5)]
        public Expression BinaryComparisonExpression(Expression left, Token<ExpressionToken> operation, Expression right)
        {
            return new BinaryOperation(left, operation.TokenID, right);
        }


        [Operand]
        [Production("operand : primary_value")]
        public Expression OperandValue(Expression value)
        {
            return value;
        }


        [Production("primary_value : IDENTIFIER")]
        public Expression OperandVariable(Token<ExpressionToken> identifier)
        {
            return new ExpressionVariable(identifier.Value);
        }

        [Production("primary_value : INT")]
        public Expression OperandInt(Token<ExpressionToken> value)
        {
            return new Number(value.IntValue);
        }

        [Production("primary_value : LPAREN SimpleExpressionParserWithContext_expressions RPAREN")]
        public Expression OperandParens(Token<ExpressionToken> lparen, Expression value, Token<ExpressionToken> rparen)
        {
            return value;
        }
    }
}