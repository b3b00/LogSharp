using logSharp.interpretation.unification;
using logSharp.model;

namespace logSharp.model
{
    public interface IExpression : IValuable
    {
        object Evaluate(Unification unification);
    }
}