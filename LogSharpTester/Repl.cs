using System;
using System.Collections.Generic;
using System.Linq;
using sly.parser;
using sly.parser.generator;
using logSharp.interpretation;
using logSharp.interpretation.unification;
using logSharp.model;
using logSharp.parser;
using sly.lexer;
using Utility.CommandLine;

namespace LogSharpTester
{
    
   
    public class PrologRepl
    {

        [Argument('v', "verbose", "verbosity")]
        private static string LogFile { get; set; }
        
        [Operands]
        private static string[] Programs { get; set; }

        delegate void ReplAction(Interpreter interpreter, Question question);

        private static Dictionary<string, ReplAction> ReplActions = new Dictionary<string, ReplAction>()
        {
            {"load", Load},
            {"trace", Trace},
            {"quiet", Quiet},
            {"dump", Dump}
        };

        static Parser<LogSharpToken,LogSharpAST> BuildParser()
        {
            var logParser = new LogSharpParser();
            var builder = new ParserBuilder<LogSharpToken, LogSharpAST>();
            var parser = builder.BuildParser(logParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "program");
            var lexer = parser.Result.Lexer as GenericLexer<LogSharpToken>;
            CallBacksBuilder.BuildCallbacks<LogSharpToken>(lexer);
            return parser.Result;
        }


        static void Load(Interpreter interpreter, Question ask)
        {
            if (ask.Parameters.Count == 1)
            {
                var param = ask.Parameters[0];
                if (param is Atom atom)
                {
                    interpreter.LoadProgramFile(atom.Name);
                }
                else if (param is logSharp.model.LogString str)
                {
                    interpreter.LoadProgramFile(str.Value);
                }
                else
                {
                    Console.WriteLine(" bas parameter");
                }
            }
            else
            {
                Console.WriteLine("bad query. load takes 1 parameter.");
            }
        }


        static void Trace(Interpreter interpreter, Question ask)
        {
            if (ask.Parameters.Count <= 1)
            {
                string outFile = null;
                if (ask.Parameters.Count == 1)
                {
                    var param = ask.Parameters[0];
                    if (param is Atom atom)
                    {
                        outFile = atom.Name;
                    }
                    else if (param is logSharp.model.LogString str)
                    {
                        outFile = str.Value;
                    }
                    else
                    {
                        Console.WriteLine(" bad parameter");
                    }
                }

                interpreter.DebugOutput = outFile;
                interpreter.Verbose = true;
            }
            else
            {
                Console.WriteLine("bad query. trace takes 0 or 1 parameter.");
            }
        }


        static void Quiet(Interpreter interpreter, Question ask)
        {
            interpreter.Verbose = false;
        }

        static void Dump(Interpreter interpreter, Question ask)
        {
            interpreter.LogSharpProgram.Rules.ForEach(r => Console.WriteLine(r));
            interpreter.BuiltIns.Keys.ToList().ForEach(b => Console.WriteLine(b));
        }


        public static void Loop(string[] args)
        {
            Arguments.Populate();
            var parser = BuildParser();
            Interpreter interpreter = new Interpreter(false, null, parser, new NestedTermUnifier());
            

            if (!string.IsNullOrEmpty(LogFile))
            {
                interpreter.DebugOutput = LogFile;
                interpreter.Verbose = true;
            }

            if (Programs != null && Programs.Length > 0)
            {
                for (int i = 1; i < Programs.Length; i++)
                {
                    if (!Programs[i].EndsWith(".dll"))
                    {
                        interpreter.LoadProgramFile(Programs[i]);
                    }
                }

            }
            while (true)
            {
                ReadLine.HistoryEnabled = true;
                string input = ReadLine.Read("?. ");
                if (input == "quit" || input == "exit")
                {
                    return;
                }
                else
                {
                    if (!string.IsNullOrEmpty(input))
                    {
                        var query = parser.Parse(input, "rule");
                        if (query.IsOk)
                        {
                            if (query.Result is Question ask)
                            {
                                if (ReplActions.ContainsKey(ask.Name))
                                {
                                    ReplActions[ask.Name](interpreter, ask);

                                }
                                else
                                {
                                    var answers = interpreter.Search(ask);

                                    foreach (var answer in answers)
                                    {
                                        Console.WriteLine(answer);
                                    }
                                }
                            }
                            else if (query.Result is Rule rule)
                            {
                                interpreter.Append(rule);
                            }
                        }
                        else
                        {
                            Console.WriteLine("bad query.");
                        }
                    }
                }
            }
        }


        private static void UnifyTest(string source, string dest, string parserRule)
        {
            var parser = BuildParser();
            IUnifier unifier = new NestedTermUnifier();
            var r1 = parser.Parse(source, parserRule);
            var r2 = parser.Parse(dest, parserRule);
            if (r1.IsOk && r2.IsOk)
            {
                var t1 = r1.Result as Term;
                var t2 = r2.Result as Term;
                var u = unifier.UnifyTerms(t1, new Unification(), t2, new Unification());
                Console.WriteLine(source);
                Console.WriteLine(dest);
                Console.WriteLine(u);
            }
        }

        public static void Test()
        {
            UnifyTest("list(a,T)","list(H,TAIL)","term");            
            UnifyTest("list(a,list(b,c))","list(H,list(HEAD,TAIL))","term"); // expect H=a HEAD=b tail = [c] | c
            UnifyTest("list(a,list(b,c))","list(H,list(HEAD,TAIL))","term");
//            "empty([]).",
////                    "const([a|Tail]).",
////                    "literal([a,b,c])."
            UnifyTest("[]","[H|T]","list");
            UnifyTest("[a,b]","[H|T]","list");
            UnifyTest("[a,b,c]","[H|T]","list");
            ;
        } 
        

        public static void Run(string[] args)
        {
//            Test();
            Loop(args);
        }
    }
}