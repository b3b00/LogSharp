using System;
using System.Collections.Generic;
using System.Linq;
using DeepCopy;
using logSharp.interpretation.unification;
using logSharp.model;
using String = logSharp.model.String;

namespace logSharp.interpretation
{
    public class Primitives
    {
        public static Unification testPrim(Interpreter interpreter, Unification unificationIn,
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
            return unificationOut;
        }

        public static Unification Member(Interpreter interpreter, Unification unificationIn,
            List<Valuable> parameters)

        {
            if (parameters[0] is Scalar scalar)
            {
               if (parameters[1].IsList && parameters[1].IsList && parameters[1].IsList && parameters[1] is Term term)
                {
                    var items = term.ToValuableList();
                    if (items.Contains(scalar))
                    {
                        return Unification.Success();
                    }
                }
            }
            return Unification.Failed();
        }

        public static Unification BagOf(Interpreter interpreter, Unification unificationIn,
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
                            var results = interpreter.Search(question,unificationIn).ToList();
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
                            unificationOut.AddBinding(outputVar,list);
                            return unificationOut;
                        }
                        else
                        {
                            Console.WriteLine($"bad query : {query.Value}");
                        }
                    }   
                }
            }

            return Unification.Failed();
        }
    }
}