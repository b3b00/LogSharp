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
        public static List<Unification> IsPrim(Term isTerm, Interpreter interpreter, Unification unificationIn,
            List<Valuable> parameters)
        {
            if (parameters[0] is Variable variable)
            {
                var p1 = parameters[1];
                if (p1 is String source)
                {
                    var parseResult = Parser.Parse(source.Value, $"{typeof(VariableExpressionParser).Name}_expressions");

                    if (parseResult.IsOk)
                    {
                        Expression expression = parseResult.Result;
                        isTerm.Data["compiled"] = expression;
                        var result = expression.Evaluate(unificationIn);
                        Unification unificationOut = new Unification();
                        Valuable value = null;
                        if (result is string stringRes)
                        {
                            value = new String(stringRes);
                        } 
                        else if (result is int intRes)
                        {
                            value = new Int(intRes);
                        }
                        else if (result is double doubleRes)
                        {
                            value = new Double(doubleRes);
                        }
                        unificationOut.AddBinding(variable,value);
                    }
                    else
                    {
                        throw new ExpressionException(ExpressionError.Error,
                            $"errors : {string.Join(" ", parseResult.Errors.Select(e => e.ErrorMessage).ToArray<string>())}");
                    }

                     
                    
                    return ListHelper.BuildList(Unification.Success());
                }
            }

            return ListHelper.BuildList(Unification.Failed());
        }
    }
}