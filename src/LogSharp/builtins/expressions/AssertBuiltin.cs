using System;
using System.Collections.Generic;
using System.Linq;
using logSharp.helpers;
using logSharp.interpretation;
using logSharp.interpretation.unification;
using logSharp.model;

namespace logSharp.builtins.expressions
{
    /// <summary>
    /// The assert/1 builtin evaluates bare expressions (e.g. "X > 0")
    /// and returns success if the expression is true, failure otherwise.
    /// For clause terms (e.g. "likes(bob, curry)") it behaves like assertz/1
    /// (appending to the predicate). This builtin is NOT for dynamic database
    /// management; use assertz/1, asserta/1, retract/1, retractall/1 instead.
    /// </summary>
    [BuiltIns]
    public class AssertBuiltin
    {

        public const string AssertBuiltinName = "assert";

        // the ":-" constructor, used to build/retrieve rule clauses
        public const string ImplyFunctor = ":-";
        private const string ConjunctionFunctor = ",";
        private const string TrueAtomName = "true";

        [BuiltIn("assert/1")]
        public static List<Unification> AssertPrim(Interpreter interpreter, Goal goal,
            List<Valuable> parameters)
        {
            if (parameters.Count == 1)
            {
                var argument = parameters[0];

                // The parser wraps bare expressions (e.g. "X > 3") into assert/1
                // goals : keep evaluating them, like the original built-in did.
                if (argument is BinaryOperation || argument is UnaryOperation)
                {
                    var result = ((IExpression)argument).Evaluate(goal.Unification);
                    if (result is bool boolRes)
                    {
                        if (boolRes)
                        {
                            return ListHelper.BuildList(Unification.Success());
                        }
                    }

                    return ListHelper.BuildList(Unification.Failed());
                }

                // otherwise assert/1 behaves like assertz/1 (ISO semantics):
                // append the clause at the end of its predicate.
                return DynamicDbManagementBuiltins.AssertZPrim(interpreter, goal, parameters);
            }

            return ListHelper.BuildList(Unification.Failed());
        }

    }
}
