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
    

    public class Program
    {

        static BuildResult<Parser<LogSharpToken,LogSharpAST>> BuildParser()
        {
            var logParser = new LogSharpParser();
            var builder = new ParserBuilder<LogSharpToken, LogSharpAST>();
            var parser = builder.BuildParser(logParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "program");
            var lexer = parser.Result.Lexer as GenericLexer<LogSharpToken>;
            CallBacksBuilder.BuildCallbacks<LogSharpToken>(lexer);
            return parser;
        }


        static void Searching()
        {
            var parserBuildResult = BuildParser();
            if (parserBuildResult.IsError)
            {
                ;
                parserBuildResult.Errors.ForEach(e => Console.WriteLine($"ERROR : {e.Level}  - {e.Message}"));
            }
            else
            {
                string program = @"
child(X) :- boy(X).
child(X) :- girl(X).
girl(alice).
boy(alex).
";
                Console.WriteLine(("OK !"));
                var parser = parserBuildResult.Result;
                
                var cp = parser.Parse(program, "program");

                Interpreter interpreter = new Interpreter(false, cp.Result as LogSharpProgram,parser,new NestedTermUnifier());
//                Ask(interpreter,parser,"child(Q)?");
//                Ask(interpreter,parser,"boy(B)?");
//                Ask(interpreter,parser,"girl(G)?");
                Ask(interpreter,parser,"ouaf(B)?");
                Ask(interpreter,parser,"boy(blop)?");
                
                ;
            }

        }

        static void Ask(Interpreter interpreter, Parser<LogSharpToken,LogSharpAST> parser, string ask)
        {
            Console.WriteLine();
            Console.WriteLine($"asking : {ask}");
            var q = parser.Parse(ask, "question");
            Question question = q.Result as Question;
            interpreter.Search(question);
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

        static void Unifyterms(Term t1, Term t2)
        {
            Unifier unifier = new Unifier();
            Console.WriteLine($"{t1.Dump()} // {t2.Dump()}");
            var res1 = unifier.UnifyTerms(t1, new Unification(), t2, new Unification());
            Console.WriteLine(res1.ToString());
        }

        static void Unify(Parser<LogSharpToken,LogSharpAST> parser)
        {
            
                var r1 = parser.Parse("boy(billy)","term");
                var billy = (Term)r1.Result;
                var r2 = parser.Parse("boy(bob)","term");
                var bob = (Term)r2.Result;
                var r3 = parser.Parse("boy(X)","term");
                var X = (Term)r3.Result;
                var r4 = parser.Parse("boy(Y)","term");
                var Y = (Term)r4.Result;
                Unifier unifier = new Unifier();
                Unifyterms(billy, bob);
                Unifyterms(billy, billy);
                Unifyterms(billy,  X);
                Unifyterms(X, Y);
                ;
        }

        static void Main(string[] args)
        {
            PrologRepl.Run(args);
        }
    }
}