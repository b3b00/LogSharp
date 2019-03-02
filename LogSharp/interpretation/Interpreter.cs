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
        public Interpreter(bool verbose, LogSharpProgram logSharpProgram, Parser<LogSharpToken, LogSharpAST> parser,
            IUnifier unifier)
        {
            LogSharpProgram = logSharpProgram;
            Verbose = verbose;
            Parser = parser;
            Unifier = unifier;
            BuiltIns = new Dictionary<string, PrologBuiltIn>();
            BuiltinsLoader.LoadBuiltIns(this);
        }


        public bool Verbose { get; set; }

        public string DebugOutput { get; set; }

        public LogSharpProgram LogSharpProgram { get; set; }

        private IUnifier Unifier { get; }

        public Parser<LogSharpToken, LogSharpAST> Parser { get; }

        public Dictionary<string, PrologBuiltIn> BuiltIns { get; set; }

        public void AddBuiltIn(string spec, PrologBuiltIn builtIn)
        {
            if (!BuiltIns.ContainsKey(spec)) BuiltIns[spec] = builtIn;
        }

        private bool TryGetBuiltIn(string spec, out PrologBuiltIn builtIn)
        {
            return BuiltIns.TryGetValue(spec, out builtIn);
        }

        public void Append(Rule rule)
        {
            if (LogSharpProgram == null) LogSharpProgram = new LogSharpProgram(new List<Rule>());

            LogSharpProgram.Append(rule);
        }

        public void Append(LogSharpProgram program)
        {
            if (LogSharpProgram == null)
                LogSharpProgram = program;
            else
                LogSharpProgram.Append(program);
        }


        public void Trace(string message)
        {
            if (Verbose)
            {
                Console.WriteLine(message);
                if (DebugOutput != null) File.AppendAllText(DebugOutput, message + "\n");
            }
        }

        public void LoadProgramFile(string program)
        {
            var prog = File.ReadAllText(program);
            LoadProgram(prog);
        }

        public void LoadProgram(string program)
        {
            var logprog = Parser.Parse(program, "program");
            if (logprog.IsOk)
            {
                Append(logprog.Result as LogSharpProgram);
            }
            else
            {
                Console.WriteLine("errors");
                foreach (var error in logprog.Errors) Console.WriteLine($"\t{error.ErrorMessage}");
            }
        }

        public IEnumerable<Unification> Search(Question question, Unification unification = null)
        {
            var resultFound = false;
            var stack = new Stack<Goal>();
            Trace($"search question {question}");
            if (unification != null && unification.IsOk)
            {
                var evaled = Unifier.EvalTerm(question, unification);
                question = new Question(evaled);
            }

            var rootGoal = new Goal(question);
            rootGoal.Goals.Add(rootGoal);
            Trace($"stck : {rootGoal}");
            stack.Push(rootGoal);
            while (stack.Any())
            {
                var stackAsList = stack.ToList();
                var current = stack.Pop();
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
                    var u = new Unification();
                    if (current.IsFact)
                        u = Unifier.UnifyTerms(current.Head, current.Unification,
                            p.Goals[p.Index].Head, p.Unification);
                    else if (current.Index >= current.Count)
                        u = Unifier.UnifyTerms(current.Head, current.Unification,
                            p.Head, p.Unification);
                    else
                        ;

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


                if (TryGetBuiltIn(term.Specification, out var builtIn))
                {
                    var builtInTerm = DeepCopier.Copy(term.Head);
                    builtInTerm = Unifier.EvalTerm(builtInTerm, current.Unification);
                    Trace($"calling built-in {term.Specification}");
                    var builtInUnifications = builtIn(this, current.Unification, builtInTerm.Parameters);
                    if (current.Rule != null)
                    {
                        if (builtInUnifications != null && builtInUnifications.Any())
                            foreach (var builtInUnification in builtInUnifications)
                            {
                                var built = new Goal(current.Rule, current, builtInUnification);
                                built.Unification.AddBindings(current.Unification);
                                built.Index = current.Index + 1;
                                Trace($"stck {built} @ {built.Index}");
                                stack.Push(built);
                            }
                    }
                    else
                    {
                        resultFound = true;
                        foreach (var builtInUnification in builtInUnifications) yield return builtInUnification;
                    }
                }
                else
                {
                    var applicableRules = LogSharpProgram.GetRulesForSpec(term.Specification);
                    foreach (var applicable in applicableRules)
                    {
                        Trace($"trying rule {applicable.Head}");
//                        if (!applicable.Equals(current.Rule))
//                        {
                        var child = new Goal(applicable, current, current.Unification);

                        var unif = Unifier.UnifyTerms(term.Head, current.Unification, child.Head,
                            child.Unification);
                        if (unif.IsOk)
                        {
                            child.Unification = unif;
                            Trace($"stck {child} @ {child.Index}");
                            stack.Push(child);
                        }
                        else
                        {
                            Trace("unification failed.");
                        }

//                        }
                    }
                }
            }

            if (resultFound)
                yield break;
            yield return Unification.Failed();
        }
    }
}