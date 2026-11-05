using System;
using System.Collections.Generic;
using System.Linq;
using logSharp.model;
using logSharp.helpers;
using logSharp.interpretation;
using logSharp.interpretation.unification;
using String = logSharp.model.String;

namespace logSharp.builtins.expressions
{
    [BuiltIns]
    public class IsBuiltin
    {

        public const string IsBuiltinName = "is"; 
        
        [BuiltIn("is/2")]
        // param 1 : variable / value ?
        // param 2 : expression as an expression tree
        public static List<Unification> IsPrim(Interpreter interpreter, Unification unificationIn,
            List<Valuable> parameters)
        {
            List<Unification> unifications = new List<Unification>();

            if (parameters.Count == 2 && parameters[0] is Variable variable && parameters[1] is IExpression expression)
            {

                var result = expression.Evaluate(unificationIn);
                Unification unificationOut = new Unification();
                Valuable value = null;
                if (result is string stringRes)
                {
                    value = new model.String(stringRes);
                }
                else if (result is int intRes)
                {
                    value = new model.Int(intRes);
                }
                else if (result is double doubleRes)
                {
                    value = new model.Double(doubleRes);
                }

                unificationOut.AddBinding(variable, value);
                unifications.Add(unificationOut);
                return unifications;
            }

            return ListHelper.BuildList(Unification.Failed());
        }
    }
}