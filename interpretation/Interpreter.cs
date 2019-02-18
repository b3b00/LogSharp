using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Data;
using System.Diagnostics;
using System.IO;
using DeepCopy;
using logSharp.interpretation.unification;
using logSharp.model;

namespace logSharp.interpretation
{
    public class Interpreter
    {
        private bool Verbose { get; set; }
        
        private LogSharpProgram LogSharpProgram { get; set; }


        public Interpreter(bool verbose, LogSharpProgram logSharpProgram)
        {
            LogSharpProgram = logSharpProgram;
            Verbose = verbose;
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
            Trace($"push : {rootGoal}");
            stack.Push(rootGoal);
            while (stack.Any())
            {
                Goal current = stack.Pop();
                Trace($"pop {current}");
                if (current.Index >= current.Count)
                {
                    if (current.IsRootGoal)
                    {
                        if (current.Unification.IsOk)
                        {
                            Console.WriteLine("Yes.");
                            Console.WriteLine(current.Unification);
                        }
                        else
                        {
                            Console.WriteLine(("No."));
                        }
                        continue;
                    }

                    var parent = DeepCopier.Copy(current.Parent);
                    
                }
            }
            return null;
        }
    }
}