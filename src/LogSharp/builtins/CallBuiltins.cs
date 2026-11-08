using System.Collections.Generic;
using System.Linq;
using logSharp.helpers;
using logSharp.interpretation;
using logSharp.interpretation.unification;
using logSharp.model;

namespace logSharp.builtins;

[BuiltIns]
public class CallBuiltins
{
    [BuiltIn("call/1")]
    public static List<Unification> Call1(Interpreter interpreter, Goal goal,
        List<Valuable> parameters)
    {
        var call = Call_X(2);
        if (parameters[0] is Term term)
        {
            List<Valuable> calleeParameters = new List<Valuable>();
            calleeParameters.Add(new Atom(term.Name));
            calleeParameters.AddRange(term.Parameters);
            return call(interpreter, goal, calleeParameters);
        }
        else
        {
            Valuable first = ListBuiltins.ResolveParameter(parameters[0], goal.Unification);
            return call(interpreter, goal, parameters);
        }
    }

    [BuiltIn("not/1")]
    public static List<Unification> Not1(Interpreter interpreter, Goal goal,
        List<Valuable> parameters)
    {

        var result = Call1(interpreter, goal, parameters);
        if (result.Any(x => x.IsOk))
        {
            return ListHelper.BuildList(Unification.Failed());
        }
        else
        {
            return ListHelper.BuildList(Unification.Success());
        }
    }

    public static PrologBuiltIn Call_X(int count)
    {
        return (Interpreter interpreter, Goal goal,
            List<Valuable> parameters) =>
        {
            if (parameters[0] is Atom innerGoal)
            {
                if (interpreter.HasRule(innerGoal.Name + $"/{count - 1}")
                    || interpreter.HasBuiltin(innerGoal.Name + $"/{count - 1}"))
                {
                    Term goalTerm = new Term(innerGoal.Name, parameters.Skip(1).ToList());
                    var question = new Question(goalTerm);
                    var results = interpreter.Search(question, goal.Unification).ToList();
                    return results;
                }
            }


            return ListHelper.BuildList(Unification.Failed());
        };
    }

    public static PrologBuiltIn Not_X(int count)
    {
        return (Interpreter interpreter, Goal goal,
            List<Valuable> parameters) =>
        {
            var call = Call_X(count);
            var result = call(interpreter, goal, parameters);
            if (result.Any(x => x.IsOk))
            {
                return ListHelper.BuildList(Unification.Failed());
            }
            else
            {
                return ListHelper.BuildList(Unification.Success());
            }
        };
    }

    [BuiltInBuilder]
    public static void BuildCallBuiltIns(Interpreter interpreter)
    {
        for (int i = 2; i < 10; i++)
        {
            interpreter.AddBuiltIn($"call/{i}", Call_X(i));
            interpreter.AddBuiltIn($"not/{i}", Not_X(i));
        }
    }
}