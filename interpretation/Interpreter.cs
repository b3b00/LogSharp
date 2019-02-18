using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DeepCopy;
using logSharp.interpretation.unification;
using logSharp.model;

namespace logSharp.interpretation
{
    public class Interpreter
    {
        private bool Verbose { get; set; }
        
        private LogSharpProgram LogSharpProgram { get; set; }
        
        private Unifier Unifier { get; set; }


        public Interpreter(bool verbose, LogSharpProgram logSharpProgram)
        {
            LogSharpProgram = logSharpProgram;
            Verbose = verbose;
            Unifier = new Unifier();
        }

        public void Trace(string message)
        {
            if (Verbose)
            {
                Console.WriteLine(message);
            }
        }
        
        public Unification Search(Question question)
        {
            Stack<Goal> stack = new Stack<Goal>();
            Trace($"search question {question}");
            Goal rootGoal = new Goal(question);
            rootGoal.Goals.Add(rootGoal);
            Trace($"push : {rootGoal.ToString()}");
            stack.Push(rootGoal);
            while (stack.Any())
            {
                var stackAsList = stack.ToList();
                Goal current = stack.Pop();
                Trace($"pop {current}");
                if (current.Index >= current.Count || current.IsFact)
                {
                    if (current.IsRootGoal)
                    {
                        if (current.Unification.IsOk)
                        {
                            Console.WriteLine(current.Unification);
                        }
                        else
                        {
                            Console.WriteLine(("No."));
                        }

                        continue;
                    }

                    var p = DeepCopier.Copy(current.Parent);
                    Unifier.UnifyTerms(current.Head, current.Unification,
                        p.Goals[p.Index].Head, p.Unification);
                    p.Index++;
                    Trace($"stck {p} @ {p.Index}");
                    stack.Push(p);
                    continue;
                }

                var parent = DeepCopier.Copy(current.Parent);
                var term = current.Goals[current.Index];
                
                List<Rule> applicableRules = LogSharpProgram.GetRulesForSpec(term.Specification);
                foreach (var applicable in (applicableRules))
                {
                    if (applicable != current.Rule)
                    {
                        Goal child = new Goal(applicable, current, current.Unification);

                        var unif = Unifier.UnifyTerms(term.Head, term.Unification, child.Head, child.Unification);
                        if (unif.IsOk)
                        {
                            child.Unification = unif;
                            Trace($"stck {child} @ {child.Index}");
                            stack.Push(child);
                        }
                    }
                }

            }


            return rootGoal.Unification;
            return null;
        }
    }
}