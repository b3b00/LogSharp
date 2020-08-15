using System;
using System.Collections.Generic;
using System.Linq;
using DeepCopy;
using logSharp.model;
using logSharp.interpretation;
using logSharp.interpretation.unification;
using static logSharp.helpers.ListHelper;

namespace logSharp.builtins
{
    [BuiltIns]
    public class PreludeBuiltins
    {
       

        [BuiltIn("member/2")]
        public static List<Unification> MemberPrim(Term memberTerm, Interpreter interpreter, Unification unificationIn,
            List<IValuable> parameters)

        {
            if (parameters[0] is Scalar scalar)
            {
                var p1 = parameters[1];
                if (p1.IsList && p1.IsList && p1.IsList && p1 is Term term)
                {
                    var items = term.ToValuableList();
                    if (items.Contains(scalar))
                    {
                        return BuildList(Unification.Success());
                    }
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
                        List<Unification> unifications = new List<Unification>();
                        foreach (var item in items)
                        {
                            Unification unification = new Unification();
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
        public static List<Unification> BagOfPrim(Term bagOfTerm, Interpreter interpreter, Unification unificationIn,
            List<IValuable> parameters)

        {
            if (parameters[0] is Variable lookedFor)
            {
                if (parameters[1] is Term query)
                {
                    if (parameters[2] is Variable outputVar)
                    {
                            Question question = new Question(query);
                            bool previousVerbosity = interpreter.Verbose;
                            interpreter.Verbose = false;
                            var results = interpreter.Search(question, unificationIn).ToList();
                            interpreter.Verbose = previousVerbosity;
                            bool fullyFailed = results.Select(r => r.IsOk)
                                .Aggregate(false, (bool b1, bool b2) => !b1 && !b2);
                            if (fullyFailed)
                            {
                                return BuildList(Unification.Failed());
                            }

                            var values = new List<IValuable>();
                            foreach (var result in results)
                            {
                                if (result != null && result.Environment.ContainsVariable(lookedFor.Name))
                                {
                                    values.Add(result.Environment.Environment[lookedFor.Name]);
                                }
                            }

                            Term list = Term.LiteralListTerm(values);
                            var unificationOut = new Unification();
                            unificationOut.IsOk = true;
                            unificationOut.AddBinding(outputVar, list);
                            return BuildList(unificationOut);
                    }
                    else if (parameters[2].IsList && parameters[2] is Term list)
                    {
                            Question question = new Question(query);
                            bool previousVerbosity = interpreter.Verbose;
                            interpreter.Verbose = false;
                            var results = interpreter.Search(question, unificationIn).ToList();
                            interpreter.Verbose = previousVerbosity;
                            bool fullyFailed = results.Select(r => r.IsOk)
                                .Aggregate(false, (bool b1, bool b2) => !b1 && !b2);
                            if (fullyFailed)
                            {
                                return BuildList(Unification.Failed());
                            }
                            var values = new List<IValuable>();
                            foreach (var result in results)
                            {
                                if (result != null && result.Environment.ContainsVariable(lookedFor.Name))
                                {
                                    values.Add(result.Environment.Environment[lookedFor.Name]);
                                }
                            }
                            // TODO check if values and list.items == 
                            List<IValuable> inValues = list.ToValuableList();
                            int intersectCount = inValues.Intersect(values).ToList().Count; 
                            if (intersectCount == inValues.Count && intersectCount == values.Count)
                            {
                                return BuildList(Unification.Success());
                            }
                            else
                            {
                                return BuildList(Unification.Failed());
                            }
                    }
                }
            }

            return BuildList(Unification.Failed());
        }
    }
}