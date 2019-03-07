using logSharp.interpretation.unification;
using logSharp.model;

namespace logSharp.model
{
    public class Group : ILogSharpAst
    {
        private readonly IExpression InnerExpression;

        public Group(IExpression expr)
        {
            InnerExpression = expr;
        }

        public object Evaluate(Unification unification)
        {
            return InnerExpression.Evaluate(unification);
        }

        public string Dump()
        {
            return $"( {InnerExpression} )";
        }
    }
}