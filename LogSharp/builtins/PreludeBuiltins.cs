using System;
using System.Collections.Generic;
using System.Linq;
using DeepCopy;
using logSharp.helpers;
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
        // member/2 : member(Needle,Haystack) : Needle is member of list Haystack 
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

        // bag_of/3 : bag_of(OutputPredicateValue,Predicate,OutputList)
        // getting list of OutputPredicateValue that match Predicate in OutputList
        [BuiltIn("bag_of/3")]
        public static List<Unification> BagOf(Interpreter interpreter, Unification unificationIn,
            List<Valuable> parameters)
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
                            var values = new List<Valuable>();
                            foreach (var result in results)
                            {
                                if (result != null && result.Environment.ContainsVariable(lookedFor.Name))
                                {
                                    values.Add(result.Environment.Environment[lookedFor.Name]);
                                }
                            }
                            // TODO check if values and list.items == 
                            List<Valuable> inValues = list.ToValuableList();
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

        // map_list/3 : map_list(InList,Mapper, OutList). mapper is mapper/2 with mapper(In,Out)
        [BuiltIn("map_list/3")]
        
        public static List<Unification> MapList(Interpreter interpreter, Unification unificationIn,
            List<Valuable> parameters)
        {
            if (parameters[0] is Term inList && inList.IsList)
            {
                if (parameters[1] is Term query && query.Arity == 2 && query.Parameters[0] is Variable In &&
                    query.Parameters[1] is Variable Out) // TODO query args are both variable
                {
                    if (parameters[2] is Variable outputVar)
                    {
                        
                        
                        bool previousVerbosity = interpreter.Verbose;
                        interpreter.Verbose = false;
                        var toBeMapped = inList.ToValuableList();
                        List<Valuable> mapped = new List<Valuable>();
                        foreach (var x in  toBeMapped)
                        {
                            var selector = DeepCopier.Copy(query);
                            selector.Parameters[0] = x;
                            var question =  new Question(selector);
                            var results = interpreter.Search(question, unificationIn).ToList();
                            if (results.Count > 0 && results[0].IsOk)
                            {
                                var value =  results[0].Environment.Environment[Out.Name];
                                mapped.Add(value);
                            }
                            else
                            {
                                return BuildList(Unification.Failed());
                            }
                        }

                        var output = Term.ListTerm(mapped);
                        var unificationOut = new Unification();
                        unificationOut.IsOk = true;
                        unificationOut.AddBinding(outputVar, output);
                        return BuildList(unificationOut);
                        
                        
                        return BuildList(Unification.Success());
                    }
                }
            }

            return BuildList(Unification.Failed());
        }
        
      
       
        
        
        public static PrologBuiltIn Call_X(int count)
        {
            return (Interpreter interpreter, Unification unificationIn,
                List<Valuable> parameters) =>
            {
                if (parameters[0] is Atom goal &&
                    interpreter.HasRule(goal.Name + $"/{count - 1}"))
                {

                    Term goalTerm = new Term(goal.Name, parameters.Skip(1).ToList());
                    var question = new Question(goalTerm);
                    var results = interpreter.Search(question, unificationIn).ToList();
                    return results;
                }

                return BuildList(Unification.Failed());
            };
        }

        [BuiltInBuilder]
        public static void BuildCallBuiltIns(Interpreter interpreter)
        {
            for (int i = 1; i < 10; i++)
            {
                interpreter.AddBuiltIn($"call/{i}", Call_X(i));    
            }
        }
    }
}