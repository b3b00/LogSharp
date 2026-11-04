using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using logSharp.interpretation;

namespace LogSharp.prelude;

public class PreludeLoader
{
    public static void LoadPrelude(Interpreter interpreter)
    {
        var assembly = typeof(PreludeLoader).Assembly;
        var preludeResourceName  =assembly.GetManifestResourceNames().FirstOrDefault(x => x.Contains("prelude.pro"));
        using (var stream = assembly.GetManifestResourceStream(preludeResourceName))
        {
            using (var reader = new StreamReader(stream))
            {
                var content =  reader.ReadToEnd();
                interpreter.LoadProgram(content);
            }
        }
        
    }
}