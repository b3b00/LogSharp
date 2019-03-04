using logSharp.interpretation.unification;

namespace logSharp.builtins.expressions.model
{
    public interface Expression
    {
        object Evaluate(Unification unification);
    }
}