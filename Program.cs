using System;
using System.Collections.Generic;
using sly.buildresult;
using sly.parser;
using sly.parser.generator;
using System.Linq;
using logSharp.interpretation;
using logSharp.interpretation.unification;
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

        static Parser<LogSharpToken,LogSharpAST> Parsing()
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

                List<string> programs = new List<string>();
//                {
//                    "fact(hello).",
//                    "factDeux(hello, world).",
//                    "factTrois(hello, world,3).",
//                    "p(X) :- fact(X).",
//                    "p(X) :- fact(X) fact(hello).",
//                    "p(X, Y) :- fact(X, world) fact(hello, Y).",
//                    "p(X, Y, Z) :- fact(X, world) fact(hello, Y) fact(Z, X).",
//                    "nested(a,list(b,c)).",
//                    "empty([]).",
//                    "const([a|Tail]).",
//                    "literal([a,b,c])."
//                };
                foreach (var p in programs)
                {
                    Console.WriteLine("--- ");
                    Console.WriteLine(p);
                    Console.WriteLine();
                    var cp = parser.Parse(p, "program");
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

                return parser;

            }

            return null;
        }

        static void Unifyterms(LogTerm t1, LogTerm t2)
        {
            Unifier unifier = new Unifier();
            Console.WriteLine($"{t1.Dump()} // {t2.Dump()}");
            var res1 = unifier.UnifyTerms(t1, new TermEnvironment(), t2, new TermEnvironment());
            Console.WriteLine(res1.ToString());
        }

        static void Unify(Parser<LogSharpToken,LogSharpAST> parser)
        {
            
                var r1 = parser.Parse("boy(billy)","term");
                var billy = (LogTerm)r1.Result;
                var r2 = parser.Parse("boy(bob)","term");
                var bob = (LogTerm)r2.Result;
                var r3 = parser.Parse("boy(X)","term");
                var X = (LogTerm)r3.Result;
                var r4 = parser.Parse("boy(Y)","term");
                var Y = (LogTerm)r4.Result;
                Unifier unifier = new Unifier();
                Unifyterms(billy, bob);
                Unifyterms(billy, billy);
                Unifyterms(billy,  X);
                Unifyterms(X, Y);
                ;
        }

        static void Main(string[] args)
        {
            var p = Parsing();
            Unify(p);
        }
    }
}