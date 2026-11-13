using System.Collections.Generic;
using logSharp.helpers;
using logSharp.interpretation;
using logSharp.interpretation.unification;
using logSharp.model;

namespace logSharp.builtins;

[BuiltIns]
public class CutBuiltin
{
    [BuiltIn("cut/0")]
    public static List<Unification> Cut(Interpreter interpreter, Goal goal,
        List<Valuable> parameters)
    {        
        interpreter.Cut(goal);
        return new List<Unification>() { goal.Unification };
    }

    [BuiltIn("fail/0")]
    public static List<Unification> Fail(Interpreter interpreter, Goal goal,List<Valuable> parameters )
    {
        return ListHelper.BuildList(Unification.Failed());
    }

}
