using System;
using System.Collections.Generic;
using System.Linq;
using DeepCopy;
using logSharp.model;
using String = logSharp.model.String;
using logSharp.interpretation;
using logSharp.interpretation.unification;
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
            Unification unificationOut = DeepCopier.Copy(unificationIn);
            unificationOut.AddBinding("SOMEDUMMYPRIMITIVEEFFECT", new Atom("Hello, World!"));
            foreach (var p in parameters)
            {
                if (p is Variable v)
                {
                    unificationOut.AddBinding(v, new Atom($"atom is >{v.Name.ToLower()}<"));
                }
            }

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
        public static List<Unification> BagOf(Interpreter interpreter, Unification unificationIn,
            List<Valuable> parameters)

        {
            if (parameters[0] is Variable lookedFor)
            {
                if (parameters[1] is String query)
                {
                    if (parameters[2] is Variable outputVar)
                    {
                        var queryParseResult = interpreter.Parser.Parse(query.Value, "term");
                        if (queryParseResult.IsOk)
                        {
                            Term term = queryParseResult.Result as Term;
                            Question question = new Question(term);
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

                            var values = new List<Valuable>();
                            foreach (var result in results)
                            {
                                if (result != null && result.Environment.ContainsVariable(lookedFor.Name))
                                {
                                    values.Add(result.Environment.Environment[lookedFor.Name]);
                                }
                            }

                            Term list = Term.ListTerm(values);
                            var unificationOut = new Unification();
                            unificationOut.IsOk = true;
                            unificationOut.AddBinding(outputVar, list);
                            return BuildList(unificationOut);
                        }
                        else
                        {
                            Console.WriteLine($"bad query : {query.Value}");
                        }
                    }
                }
            }

            return BuildList(Unification.Failed());
        }
    }
}