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
    public class PrologBuiltins
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
                        
                    }
                }
            }

            return BuildList(Unification.Failed());
        }
        
      
        // length/2 : length(list,len) len is length of list
        // supports 2 ways binding :
        //   way 1 : length(boundList, ???) -> compute length, bind or check second argument
        //   way 2 : length(freeList, boundLength) -> generate a list of BoundLength fresh variables
        [BuiltIn("length/2")]
        public static List<Unification> Length(Interpreter interpreter, Unification unificationIn,
            List<Valuable> parameters)
        {
            Valuable first = ResolveParameter(parameters[0], unificationIn);
            Valuable second = ResolveParameter(parameters[1], unificationIn);

            // way 1 : list is bound -> compute length, bind or check second argument
            if (first is Term inList && inList.IsList)
            {
                int realLength = inList.ToValuableList().Count;

                // way 1a : length is a free variable -> bind it
                if (second is Variable lengthOut)
                {
                    var unificationOut = new Unification();
                    unificationOut.AddBinding(lengthOut.Name, new Int(realLength));
                    return BuildList(unificationOut);
                }

                // way 1b : length is already bound -> check it
                if (second is Int expectedLength)
                {
                    if (expectedLength.Value == realLength)
                    {
                        return BuildList(Unification.Success());
                    }

                    return BuildList(Unification.Failed());
                }
            }

            // way 2 : length is bound and list is a free variable -> generate list of fresh variables
            if (second is Int lengthIn && lengthIn.Value >= 0 && first is Variable list)
            {
                if (list.IsDiscardableVariable)
                {
                    return BuildList(Unification.Success());
                }

                Term generated = Term.EmptyListTerm();
                for (int i = lengthIn.Value; i >= 1; i--)
                {
                    generated = Term.ListTerm(new Variable(BuildFreshVariableName(list.Name, i)), generated);
                }

                var unificationOut = new Unification();
                unificationOut.AddBinding(list.Name, generated);
                return BuildList(unificationOut);
            }

            return BuildList(Unification.Failed());
        }

        // sort/2 : sort(list,sorted) sorted is list sorted and free of duplicates
        // supports 2 ways binding :
        //   way 1 : sort(boundList, ???) -> sort list, bind or check second argument
        //   way 2 : sort(freeList, boundSortedList) -> bind free variable to its sorted value
        [BuiltIn("sort/2")]
        public static List<Unification> Sort(Interpreter interpreter, Unification unificationIn,
            List<Valuable> parameters)
        {
            Valuable first = ResolveParameter(parameters[0], unificationIn);
            Valuable second = ResolveParameter(parameters[1], unificationIn);

            // way 1 : list is bound -> sort it, bind or check second argument
            if (first is Term inList && inList.IsList)
            {
                List<Valuable> sortedItems = SortValues(inList.ToValuableList());
                Term sortedList = Term.ListTerm(sortedItems);

                // way 1a : sorted list is a free variable -> bind it
                if (second is Variable sortedOut)
                {
                    var unificationOut = new Unification();
                    unificationOut.AddBinding(sortedOut.Name, sortedList);
                    return BuildList(unificationOut);
                }

                // way 1b : sorted list is already bound -> check it
                if (second is Term expected && expected.IsList)
                {
                    if (AreSameList(sortedItems, expected.ToValuableList()))
                    {
                        return BuildList(Unification.Success());
                    }

                    return BuildList(Unification.Failed());
                }
            }

            // way 2 : sorted list is bound and list is a free variable
            // the only canonical input is the sorted list itself (sorted and duplicate free)
            if (second is Term boundOutput && boundOutput.IsList && first is Variable input)
            {
                List<Valuable> items = boundOutput.ToValuableList();
                if (AreSameList(SortValues(items), items))
                {
                    if (input.IsDiscardableVariable)
                    {
                        return BuildList(Unification.Success());
                    }

                    var unificationOut = new Unification();
                    unificationOut.AddBinding(input.Name, boundOutput);
                    return BuildList(unificationOut);
                }
            }

            return BuildList(Unification.Failed());
        }

        // replaces a parameter variable by its bound value when it already carries a binding
        private static Valuable ResolveParameter(Valuable parameter, Unification unification)
        {
            if (parameter is Variable variable && variable.Name != Variable.DiscardingVariable
                && unification.HasBinding(variable.Name))
            {
                return unification.GetBinding(variable.Name);
            }

            return parameter;
        }

        // sorts and removes duplicated values
        private static List<Valuable> SortValues(List<Valuable> items)
        {
            List<Valuable> sorted = new List<Valuable>(items);
            sorted.Sort(CompareValuables);

            // List<Valuable> distinct = new List<Valuable>();
            // foreach (var item in sorted)
            // {
            //     if (!distinct.Any(d => CompareValuables(d, item) == 0))
            //     {
            //         distinct.Add(item);
            //     }
            // }

            return sorted;
        }

        // numbers first, then values ordered by their dumping
        private static int CompareValuables(Valuable x, Valuable y)
        {
            if (x is Int xInt)
            {
                return y is Int yInt ? xInt.Value.CompareTo(yInt.Value) : -1;
            }

            if (y is Int)
            {
                return 1;
            }

            return string.CompareOrdinal(x.Dump(), y.Dump());
        }

        private static bool AreSameList(List<Valuable> left, List<Valuable> right)
        {
            if (left.Count != right.Count)
            {
                return false;
            }

            for (int i = 0; i < left.Count; i++)
            {
                if (CompareValuables(left[i], right[i]) != 0)
                {
                    return false;
                }
            }

            return true;
        }

        // fresh variables created by builtins are prefixed with __ to avoid name clashes with parsed programs
        private static string BuildFreshVariableName(string variableName, int index)
        {
            return $"__{variableName}_length_{index}";
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