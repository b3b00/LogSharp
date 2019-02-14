using System;
using System.Collections.Generic;
using sly.buildresult;
using sly.parser;
using sly.parser.generator;
using System.Linq;
using logSharp.model;
using logSharp.parser;
using sly.lexer;

namespace logSharp
{
    class Program
    {

        static BuildResult<Parser<LogSharpToken,LogSharpAST>> BuildParser()
        {
            var logParser = new LogSharpParser();
            var builder = new ParserBuilder<LogSharpToken, LogSharpAST>();
            var parser = builder.BuildParser(logParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "program");
            return parser;
        }
        
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var parserBuildResult = BuildParser();
            if (parserBuildResult.IsError)
            {
                ;
                parserBuildResult.Errors.ForEach(e => Console.WriteLine($"ERROR : {e.Level}  - {e.Message}"));
            }
            else
            {
                Console.WriteLine(("OK !"));

                var parser = parserBuildResult.Result;
                var lexer = parserBuildResult.Result.Lexer as GenericLexer<LogSharpToken>;
                CallBacksBuilder.BuildCallbacks<LogSharpToken>(lexer);

                List<string> programs = new List<string>()
                {
                    "fact(hello).",
                    "factDeux(hello, world).",
                    "factTrois(hello, world,3).",
                    "p(X) :- fact(X).",
                    "p(X) :- fact(X) fact(hello).",
                    "p(X, Y) :- fact(X, world) fact(hello, Y).",
                    "p(X, Y, Z) :- fact(X, world) fact(hello, Y) fact(Z, X)."
                };
                foreach (var p in programs)
                {
                    Console.WriteLine("--- ");
                    Console.WriteLine(p);
                    Console.WriteLine();
                    var cp = parser.Parse(p,"program");
                    if (cp.IsError)
                    {
                        Console.WriteLine("KO !");
                        cp.Errors.ForEach(e => Console.WriteLine($"ERROR : {e.ErrorMessage}"));
                    }
                    else
                    {
                        Console.WriteLine("OK !");
                        Console.WriteLine(cp.Result.Dump());
                    }
                }

            }
        }
    }
}