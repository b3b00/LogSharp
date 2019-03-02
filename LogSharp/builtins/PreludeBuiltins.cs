using System.Collections.Generic;
using System.Linq;
using DeepCopy;
using logSharp.interpretation;
using logSharp.interpretation.unification;
using logSharp.model;
using static logSharp.helpers.ListHelper;

namespace logSharp.builtins
{
    [BuiltIns]
    public class PreludeBuiltins
    {
        [BuiltIn("testPrim/0")]
        public static List<Unification> TestPrim(Interpreter interpreter, Unification unificationIn,
            List<Valuable> parameters)

        {
            var unificationOut = DeepCopier.Copy(unificationIn);
            unificationOut.AddBinding("SOMEDUMMYPRIMITIVEEFFECT", new Atom("Hello, World!"));
            foreach (var p in parameters)
                if (p is Variable v)
                    unificationOut.AddBinding(v, new Atom($"atom is >{v.Name.ToLower()}<"));

            ;
            return BuildList(unificationOut);
        }

        [BuiltIn("member/2")]
        public static List<Unification> Member(Interpreter interpreter, Unification unificationIn,
            List<Valuable> parameters)

        {
            if (parameters[0] is Scalar scalar)
            {
                var p1 = parameters[1];
                if (p1.IsList && p1.IsList && p1.IsList && p1 is Term term)
                {
                    var items = term.ToValuableList();
                    if (items.Contains(scalar)) return BuildList(Unification.Success());
                }
            }
            else if (parameters[0] is Variable variable)
            {
                var p1 = parameters[1];
                if (p1.IsList && p1.IsList && p1.IsList && p1 is Term term)
                {
                    var items = term.ToValuableList();
                    if (items != null && items.Any())
                    {
                        var unifications = new List<Unification>();
                        foreach (var item in items)
                        {
                            var unification = new Unification();
                            unification.AddBinding(variable.Name, item);
                            unifications.Add(unification);
                        }

                        return unifications;
                    }
                }
            }

            return BuildList(Unification.Failed());
        }

        [BuiltIn("bag_of/3")]
        public static List<Unification> BagOf(Interpreter interpreter, Unification unificationIn,
            List<Valuable> parameters)

        {
            if (parameters[0] is Variable lookedFor)
                if (parameters[1] is Term query)
                {
                    if (parameters[2] is Variable outputVar)
                    {
                        var question = new Question(query);
                        var previousVerbosity = interpreter.Verbose;
                        interpreter.Verbose = false;
                        var results = interpreter.Search(question, unificationIn).ToList();
                        interpreter.Verbose = previousVerbosity;
                        var fullyFailed = results.Select(r => r.IsOk)
                            .Aggregate(false, (b1, b2) => !b1 && !b2);
                        if (fullyFailed) return BuildList(Unification.Failed());

                        var values = new List<Valuable>();
                        foreach (var result in results)
                            if (result != null && result.Environment.ContainsVariable(lookedFor.Name))
                                values.Add(result.Environment.Environment[lookedFor.Name]);

                        var list = Term.ListTerm(values);
                        var unificationOut = new Unification();
                        unificationOut.IsOk = true;
                        unificationOut.AddBinding(outputVar, list);
                        return BuildList(unificationOut);
                    }
                    else if (parameters[2].IsList && parameters[2] is Term list)
                    {
                        var question = new Question(query);
                        var previousVerbosity = interpreter.Verbose;
                        interpreter.Verbose = false;
                        var results = interpreter.Search(question, unificationIn).ToList();
                        interpreter.Verbose = previousVerbosity;
                        var fullyFailed = results.Select(r => r.IsOk)
                            .Aggregate(false, (b1, b2) => !b1 && !b2);
                        if (fullyFailed) return BuildList(Unification.Failed());
                        var values = new List<Valuable>();
                        foreach (var result in results)
                            if (result != null && result.Environment.ContainsVariable(lookedFor.Name))
                                values.Add(result.Environment.Environment[lookedFor.Name]);
                        // TODO check if values and list.items == 
                        var inValues = list.ToValuableList();
                        var intersectCount = inValues.Intersect(values).ToList().Count;
                        if (intersectCount == inValues.Count && intersectCount == values.Count)
                            return BuildList(Unification.Success());
                        return BuildList(Unification.Failed());
                    }
                }

            return BuildList(Unification.Failed());
        }
    }
}