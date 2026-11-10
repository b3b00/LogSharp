using System.Collections.Generic;
using System.Linq;
using logSharp.helpers;
using logSharp.interpretation;
using logSharp.interpretation.unification;
using logSharp.model;

namespace logSharp.builtins;

[BuiltIns]
public class DcgBuiltins
{
    [BuiltIn("phrase/2")]
    public static List<Unification> Phrase(Interpreter interpreter, Goal goal, List<Valuable> parameters)
    {
        if (parameters.Count != 2 || parameters[0] is not Term grammarRule)
        {
            return ListHelper.BuildList(Unification.Failed());
        }

        var dcgParameters = grammarRule.Parameters
            .Concat(new[] { parameters[1], Term.EmptyListTerm() })
            .ToList();
        var question = new Question(new Term(grammarRule.Name, dcgParameters));

        return interpreter.Search(question, goal.Unification).ToList();
    }
}
