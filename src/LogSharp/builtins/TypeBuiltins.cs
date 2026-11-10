using System.Collections.Generic;
using logSharp.helpers;
using logSharp.interpretation;
using logSharp.interpretation.unification;
using logSharp.model;

namespace logSharp.builtins;

[BuiltIns]
public class TypeBuiltins
{
    [BuiltIn("atom/1")]
    public static List<Unification> Atom(Interpreter interpreter, Goal goal, List<Valuable> parameters)
    {
        return IsType<Atom>(goal, parameters);
    }

    [BuiltIn("number/1")]
    public static List<Unification> Number(Interpreter interpreter, Goal goal, List<Valuable> parameters)
    {
        var value = ResolveParameter(goal, parameters);
        return Result(value is Int or model.Double);
    }

    private static List<Unification> IsType<T>(Goal goal, List<Valuable> parameters)
        where T : Valuable
    {
        return Result(ResolveParameter(goal, parameters) is T);
    }

    private static Valuable ResolveParameter(Goal goal, List<Valuable> parameters)
    {
        return parameters.Count == 1
            ? ListBuiltins.ResolveParameter(parameters[0], goal.Unification)
            : null;
    }

    private static List<Unification> Result(bool success)
    {
        return ListHelper.BuildList(success ? Unification.Success() : Unification.Failed());
    }
}
