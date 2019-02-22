using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeepCopy;
using logSharp.builtins;
using logSharp.interpretation.unification;
using logSharp.model;
using logSharp.parser;
using sly.parser;

namespace logSharp.interpretation
{

    public delegate List<Unification> PrologBuiltIn(Interpreter interpreter, Unification unificationIn,
        List<Valuable> parameters);
    
    public class Interpreter
    {

       
        
        public bool Verbose { get; set; }
        
        public string DebugOutput { get; set; }
        
        public LogSharpProgram LogSharpProgram { get; set; }
        
        private IUnifier Unifier { get; set; }
        
        public Parser<LogSharpToken,LogSharpAST> Parser { get; private set; }
        
        private Dictionary<string,PrologBuiltIn> Primitives { get; set; }

        
        public Interpreter(bool verbose, LogSharpProgram logSharpProgram, Parser<LogSharpToken,LogSharpAST> parser, IUnifier unifier)
        {
            LogSharpProgram = logSharpProgram;
            Verbose = verbose;
            Parser = parser;
            Unifier = unifier;
            Primitives = new Dictionary<string, PrologBuiltIn>();
            BuiltinsLoader.LoadBuiltIns(this);
        }

        public void AddPrimitive(string spec, PrologBuiltIn builtIn)
        {
            if (!Primitives.ContainsKey(spec))
            {
                Primitives[spec] = builtIn;
            }
        }

        public bool tryeGetPrimitive(string spec, out PrologBuiltIn builtIn)
        {
            return Primitives.TryGetValue(spec, out builtIn);
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
        
        public IEnumerable<Unification> Search(Question question, Unification unification = null)
        {
            bool resultFound = false;
            Stack<Goal> stack = new Stack<Goal>();
            Trace($"search question {question}");
            if (unification != null && unification.IsOk)
            {
                Term evaled = Unifier.EvalTerm(question,unification);
                question = new Question(evaled);
            }
            Goal rootGoal = new Goal(question);
            rootGoal.Goals.Add(rootGoal);
            Trace($"stck : {rootGoal.ToString()}");
            stack.Push(rootGoal);
            while (stack.Any())
            {
                var stackAsList = stack.ToList();
                Goal current = stack.Pop();
                Trace("");
                Trace("____________________________");
                Trace("____________________________");
                Trace("");
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


                PrologBuiltIn builtIn;
                if (tryeGetPrimitive(term.Specification, out builtIn))
                {
                    var primitiveTerm = DeepCopier.Copy(term.Head);
                    primitiveTerm = Unifier.EvalTerm(primitiveTerm,current.Unification);
                    Trace($"calling primitive {term.Specification}");
                    var primitiveUnifications = builtIn(this, current.Unification, primitiveTerm.Parameters);
                    if (current.Rule != null) {
                        if (primitiveUnifications != null && primitiveUnifications.Any())
                        {
                            foreach (var primitiveUnification in primitiveUnifications)
                            {
                                Goal primitived = new Goal(current.Rule, current, primitiveUnification);
                                primitived.Unification.AddBindings(current.Unification);
                                primitived.Index = current.Index + 1;
                                stack.Push(primitived);
                            }
                        }
                    }
                    else
                    {
                        resultFound = true;
                        foreach (var primitiveUnification in primitiveUnifications)
                        {
                            yield return primitiveUnification;
                        }
                    }
                }
                else
                {
                    List<Rule> applicableRules = LogSharpProgram.GetRulesForSpec(term.Specification);
                    foreach (var applicable in (applicableRules))
                    {
                        Trace($"trying rule {applicable.Head}");
//                        if (!applicable.Equals(current.Rule))
//                        {
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
                            Trace("unification failed.");
//                        }
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