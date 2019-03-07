using System.Collections.Generic;
using logSharp.model;
using logSharp.helpers;
using logSharp.interpretation;
using logSharp.interpretation.unification;

namespace logSharp.builtins.expressions
{
    [BuiltIns]
    public class AssertBuiltin
    {





        [BuiltIn("assert/1")]
        // param 1 : variable / value ?
        // param 2 : expression a a string
        public static List<Unification> AssertPrim(Term term, Interpreter interpreter, Unification unificationIn,
            List<IValuable> parameters)
        {
            List<Unification> unifications = new List<Unification>();


            if (term.Parameters.Count == 1 && term.Parameters[0] is IExpression expression)
            {
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


            return ListHelper.BuildList(Unification.Failed());
        }

    }
}