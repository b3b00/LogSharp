namespace logSharp.builtins.expressions.model
{
    public interface Expression
    {
        int? Evaluate(ExpressionContext context);
    }
}