using logSharp.interpretation.unification;
using logSharp.model;

namespace logSharp.model
{
    public interface IExpression : Valuable
    {
        object Evaluate(Unification unification);
    }
}