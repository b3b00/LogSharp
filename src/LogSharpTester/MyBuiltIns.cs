using logSharp.builtins;
using logSharp.interpretation;
using logSharp.model;
using logSharp.interpretation.unification;
using System.Collections.Generic;
using static logSharp.helpers.ListHelper;

namespace LogSharpTester
{
    [BuiltIns]
    public class MyBuiltIns
    {
        [BuiltIn("testBuiltIn/0")]
        public static List<Unification> TestPrim(Interpreter interpreter, Unification unificationIn,
            List<Valuable> parameters)

        {
            return BuildList(Unification.Success());
        }
    }
}