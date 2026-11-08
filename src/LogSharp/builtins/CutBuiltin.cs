using System.Collections.Generic;
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
        
        return new List<Unification>() { goal.Unification };
    }

}