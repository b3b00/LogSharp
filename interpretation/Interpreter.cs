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

    public delegate Unification PrologPrimitive(Interpreter interpreter, Unification unificationIn,
        List<Valuable> parameters);
    
    public class Interpreter
    {

       
        
        public bool Verbose { get; set; }
        
        public string DebugOutput { get; set; }
        
        public LogSharpProgram LogSharpProgram { get; set; }
        
        private IUnifier Unifier { get; set; }
        
        public Parser<LogSharpToken,LogSharpAST> Parser { get; private set; }
        
        private Dictionary<string,PrologPrimitive> Primitives { get; set; }

        
        public Interpreter(bool verbose, LogSharpProgram logSharpProgram, Parser<LogSharpToken,LogSharpAST> parser, IUnifier unifier)
        {
            LogSharpProgram = logSharpProgram;
            Verbose = verbose;
            Parser = parser;
            Unifier = unifier;
            Primitives = new Dictionary<string, PrologPrimitive>();
        }

        public void AddPrimitive(string spec, PrologPrimitive primitive)
        {
            if (!Primitives.ContainsKey(spec))
            {
                Primitives[spec] = primitive;
            }
        }

        public bool tryeGetPrimitive(string spec, out PrologPrimitive primitive)
        {
            return Primitives.TryGetValue(spec, out primitive);
        }

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
            bool resultFound = false;
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
                            resultFound = true;
                            yield return current.Unification;
                        }

                        continue;
                    }

                    var p = DeepCopier.Copy(current.Parent);
                    var u = Unifier.UnifyTerms(current.Head, current.Unification,
                        p.Goals[p.Index].Head, p.Unification);
                    if (u.IsOk)
                    {
                        p.Index++;
                        p.Unification = u;
                        Trace($"stck {p} @ {p.Index}");
                        stack.Push(p);
                    }
                    continue;
                }

                var parent = DeepCopier.Copy(current.Parent);
                var term = current.Goals[current.Index];


                PrologPrimitive primitive;
                if (tryeGetPrimitive(term.Specification, out primitive))
                {
                    var primitiveUnification = primitive(this, current.Unification, current.Head.Parameters);
                    if (current.Rule != null)
                    {
                        Goal primitived = new Goal(current.Rule, current, primitiveUnification);
                        primitived.Index++;
                        stack.Push(primitived);
                    }
                    else
                    {
                        resultFound = true;
                        yield return primitiveUnification;
                    }
                }
                else
                {
                    List<Rule> applicableRules = LogSharpProgram.GetRulesForSpec(term.Specification);
                    foreach (var applicable in (applicableRules))
                    {
                        if (!applicable.Equals(current.Rule))
                        {
                            Goal child = new Goal(applicable, current, current.Unification);

                            //var unif = Unifier.UnifyTerms(term.Head, term.Unification, child.Head, child.Unification);
                            var unif = Unifier.UnifyTerms(term.Head, current.Unification, child.Head,
                                child.Unification);
                            if (unif.IsOk)
                            {
                                child.Unification = unif;
                                Trace($"stck {child} @ {child.Index}");
                                stack.Push(child);
                            }
                        }
                    }
                }

            }

            if (resultFound)
            {
                yield break;
            }
            else
            {
                yield return Unification.Failed();
            }
        }
    }
}