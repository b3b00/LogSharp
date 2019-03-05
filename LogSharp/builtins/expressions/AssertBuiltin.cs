using System.Collections.Generic;
using System.Linq;
using logSharp.builtins.expressions.model;
using logSharp.helpers;
using logSharp.interpretation;
using logSharp.interpretation.unification;
using logSharp.model;
using sly.parser;
using sly.parser.generator;

namespace logSharp.builtins.expressions
{
    [BuiltIns]
    public class AssertBuiltin
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



        [BuiltIn("assert/1")]
        // param 1 : variable / value ?
        // param 2 : expression a a string
        public static List<Unification> AssertPrim(Term isTerm, Interpreter interpreter, Unification unificationIn,
            List<IValuable> parameters)
        {
            List<Unification> unifications = new List<Unification>();

            if (parameters[0] is LogString source)
            {

                var parseResult = Parser.Parse(source.Value, $"{typeof(VariableExpressionParser).Name}_expressions");

                if (parseResult.IsOk)
                {
                    Expression expression = parseResult.Result;
                    isTerm.Data["compiled"] = expression;
                    var result = expression.Evaluate(unificationIn);
                    Unification unificationOut = new Unification();
                    IValuable value = null;
                    if (result is bool boolRes)
                    {
                        if (boolRes)
                        {
                            return ListHelper.BuildList(Unification.Success());
                        }
                    }
                }
                else
                {
                    throw new ExpressionException(ExpressionError.Error,
                        $"errors : {string.Join(" ", parseResult.Errors.Select(e => e.ErrorMessage).ToArray<string>())}");
                }
            }
            return ListHelper.BuildList(Unification.Failed());
        }

    }
}