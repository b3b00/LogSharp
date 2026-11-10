using logSharp.parser;

namespace logSharp.model;

public class OperatorsHelper
{
    public static string TokenToOperator(LogSharpToken token) => token switch
    {
        LogSharpToken.Different => "!=",
        LogSharpToken.NotUnifiable => "\\=",
        LogSharpToken.EqualsTo => "==",
        LogSharpToken.Gt => ">",
        LogSharpToken.Gte => ">=",
        LogSharpToken.Lt => "<",
        LogSharpToken.Lte => "<=",
        LogSharpToken.Times => "*",
        LogSharpToken.Divide => "/",
        LogSharpToken.Plus => "+",
        LogSharpToken.Minus => "-",
        _ => throw new ExpressionException(ExpressionError.Error, $"unknown operator {token}")
    };


}