using logSharp.interpretation;

namespace logSharp.builtins
{
    public class BuiltinsLoader
    {
        public static void LoadBuiltIns(Interpreter interpreter)
        {
            interpreter.AddPrimitive("testPrim/2",builtins.PreludeBuiltins.TestPrim);
            interpreter.AddPrimitive("bag_of/3",builtins.PreludeBuiltins.BagOf);
            interpreter.AddPrimitive("member/2",builtins.PreludeBuiltins.Member);
        }
    }
}