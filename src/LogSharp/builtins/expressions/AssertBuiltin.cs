using System;
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

        public const string AssertBuiltinName = "assert";



        [BuiltIn("assert/1")]
        // param 1 : variable / value ?
        // param 2 : expression a a string
        public static List<Unification> AssertPrim(Interpreter interpreter, Goal goal,
            List<Valuable> parameters)
        {
            List<Unification> unifications = new List<Unification>();


            if (parameters.Count == 1 && parameters[0] is IExpression expression)
            {
                var result = expression.Evaluate(goal.Unification);
                Unification unificationOut = new Unification();
                if (result is bool boolRes)
                {
                    if (boolRes)
                    {
                        return ListHelper.BuildList(Unification.Success());
                    }
                }
                Console.WriteLine($"assert {expression.Dump()} failed.");
            }

            
            return ListHelper.BuildList(Unification.Failed());
        }

    }
}