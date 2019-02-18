using System;
using System.Collections.Generic;
using sly.parser;
using sly.parser.generator;
using System.Linq;
using logSharp.interpretation;
using logSharp.model;
using logSharp.parser;
using sly.lexer;

namespace logSharp
{
    
   
    public class PrologRepl
    {

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
                    interpreter.LoadProgram(atom.Name);
                }
                else if (param is logSharp.model.String str)
                {
                    interpreter.LoadProgram(str.Value);
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
                    else if (param is logSharp.model.String str)
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
        }


        static void Loop(string[] args)
        {
            var parser = BuildParser();
            Interpreter interpreter = new Interpreter(false, null, parser);

            if (args.Length > 0)
            {
                foreach (var prog in args)
                {
                    interpreter.LoadProgram(prog);
                }
                
            }
            while (true)
            {
                string input = ReadLine.Read("?. ");
                if (input == "quit" || input == "exit")
                {
                    return;
                }
                else
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
                                if (answers != null && answers.Any())
                                {
                                    foreach (var answer in answers)
                                    {
                                        Console.WriteLine(answer);
                                    }
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


        

        public static void Run(string[] args)
        {
            
            Loop(args);
        }
    }
}