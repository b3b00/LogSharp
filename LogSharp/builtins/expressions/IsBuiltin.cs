using System.Collections.Generic;
using logSharp.builtins.expressions.model;
using logSharp.helpers;
using logSharp.interpretation;
using logSharp.interpretation.unification;
using logSharp.model;
using sly.parser;
using sly.parser.generator;

namespace logSharp.builtins.expressions
{
    public class IsBuiltin
    {

        private static Parser<ExpressionToken, Expression> parser;

        public static Parser<ExpressionToken, Expression> Parser
        {
            get
            {
                if (parser == null)
                {
                    var startingRule = $"{typeof(VariableExpressionParser).Name}_expressions";
                    var parserInstance = new VariableExpressionParser();
                    var builder = new ParserBuilder<ExpressionToken, Expression>();
                    parser = builder.BuildParser(parserInstance, ParserType.LL_RECURSIVE_DESCENT, startingRule).Result;
                }

                return parser;
            }
        }



        [BuiltIn("is/2")]
        // param 1 : variable / value ?
        // param 2 : expression a a string
        public static List<Unification> IsPrim(Interpreter interpreter, Unification unificationIn,
            List<Valuable> parameters)
        {
            if (parameters[0] is Variable variable)
            {
                var p1 = parameters[1];
                if (p1 is String)
                {
                    return ListHelper.BuildList(Unification.Success());
                }
            }

            return ListHelper.BuildList(Unification.Failed());
        }
    }
}