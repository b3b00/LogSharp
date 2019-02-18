using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using DeepCopy;
using logSharp.interpretation.unification;
using logSharp.model;
using logSharp.parser;
using sly.parser;

namespace logSharp.interpretation
{
    public class Interpreter
    {
        public bool Verbose { get; set; }
        
        public string DebugOutput { get; set; }
        
        public LogSharpProgram LogSharpProgram { get; set; }
        
        private IUnifier Unifier { get; set; }
        
        private Parser<LogSharpToken,LogSharpAST> Parser { get; set; }


        public void Append(Rule rule)
        {
            if (LogSharpProgram == null)
            {
                
                LogSharpProgram = new LogSharpProgram(new List<Rule>());
            }
            
            LogSharpProgram.Append(rule);
        }
        
        public void Append(LogSharpProgram program)
        {
            if (LogSharpProgram == null)
            {
                LogSharpProgram = program;
            }
            else
            {
                LogSharpProgram.Append(program);
            }
        }

        public Interpreter(bool verbose, LogSharpProgram logSharpProgram, Parser<LogSharpToken,LogSharpAST> parser)
        {
            LogSharpProgram = logSharpProgram;
            Verbose = verbose;
            Parser = parser;
            Unifier = new Unifier();
        }

        public void Trace(string message)
        {
            if (Verbose)
            {
                Console.WriteLine(message);
                if (DebugOutput != null)
                {
                    File.AppendAllText(DebugOutput, message + "\n");
                }
            }
        }

        public void LoadProgram(string program)
        {
            string prog = File.ReadAllText(program);
            var logprog = Parser.Parse(prog,"program");
            if (logprog.IsOk)
            {
                Append(logprog.Result as LogSharpProgram);
            }
            else
            {
                Console.WriteLine("errors");
                foreach (var error in logprog.Errors)
                {
                    Console.WriteLine($"\t{error.ErrorMessage}");
                }
            }
        }
        
        public IEnumerable<Unification> Search(Question question)
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
                            yield return current.Unification;
                        }
                        else
                        {
                            Console.WriteLine(("No."));
                        }

                        continue;
                    }

                    var p = DeepCopier.Copy(current.Parent);
                    var u = Unifier.UnifyTerms(current.Head, current.Unification,
                        p.Goals[p.Index].Head, p.Unification);
                    p.Index++;
                    p.Unification = u;
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

            yield return null;
        }
    }
}