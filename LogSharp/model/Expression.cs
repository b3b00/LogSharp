using logSharp.interpretation.unification;
using logSharp.model;

namespace logSharp.model
{
    public interface IExpression : ILogSharpAst
    {
        object Evaluate(Unification unification);
    }
}