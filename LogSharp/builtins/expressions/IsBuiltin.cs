using System.Collections.Generic;
using System.Linq;
using logSharp.model;
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

        [BuiltIn("is/2")]
        // param 1 : variable / value ?
        // param 2 : expression a a string
        public static List<Unification> IsPrim(Term term, Interpreter interpreter, Unification unificationIn,
            List<IValuable> parameters)
        {
            List<Unification> unifications = new List<Unification>();

            if (parameters[0] is Variable variable)
            {
                if ((term is Question question))
                {
                    if (question.ParentTerm is IsTerm isTerm)
                    {
                        var result = isTerm.Expression.Evaluate(unificationIn);
                        Unification unificationOut = new Unification();
                        IValuable value = null;
                        if (result is string stringRes)
                        {
                            value = new LogString(stringRes);
                        }
                        else if (result is int intRes)
                        {
                            value = new LogInt(intRes);
                        }
                        else if (result is double doubleRes)
                        {
                            value = new LogDouble(doubleRes);
                        }

                        unificationOut.AddBinding(variable, value);
                        unifications.Add(unificationOut);
                    }
                }

                return unifications;
            }


            return ListHelper.BuildList(Unification.Failed());
        }
    }
}