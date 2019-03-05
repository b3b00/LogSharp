using System;
using System.Linq;
using System.Text;
using logSharp.helpers;
using logSharp.interpretation;
using logSharp.interpretation.unification;
using logSharp.model;
using logSharp.parser;
using LogSharpTester;
using sly.lexer;
using sly.parser;
using sly.parser.generator;

namespace Tester
{
    class Program
    {
        
        public static Parser<LogSharpToken,LogSharpAST> BuildParser()
        {
            var logParser = new LogSharpParser();
            var builder = new ParserBuilder<LogSharpToken, LogSharpAST>();
            var parser = builder.BuildParser(logParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "program");
            var lexer = parser.Result.Lexer as GenericLexer<LogSharpToken>;
            CallBacksBuilder.BuildCallbacks<LogSharpToken>(lexer);
            return parser.Result;
        }


        static void Is()
        {
            string program = @"
            plusone(X,Y) :- is(X,""1 + Y""). 
";
            var parser = BuildParser();
            var parsed = parser.Parse(program);
            var t = parser.Parse(@"plusone(A,1)?");
            if (parsed.IsOk)
            {
                Interpreter interpreter = new Interpreter(false,parsed.Result as LogSharpProgram, parser, new NestedTermUnifier());
                
                var r = interpreter.Search("plusone(K,41)?").ToList();
                r.ForEach(u => Console.WriteLine(u.ToString())); 
                ;
            }



        }
        
        
        static void RecursiveUnificationTest()
        {
            var parser = BuildParser();
            var src = parser.Parse("member_of(X, L)", "term").Result as Term;
            var dest = parser.Parse("member_of(X, [_, L])", "term").Result as Term;
            

            var unifier = new NestedTermUnifier();

            Unification srcEnv = new Unification();
            srcEnv.AddBinding("X",new Atom("c") );
            srcEnv.AddBinding("L",Term.ListTerm(ListHelper.BuildList<Valuable>(new Atom("b"),new Atom("c") )));

            
            var destEnv = new Unification();
            var emptyChildU = new Unification();
            
            destEnv.AddBinding("X",new Atom("c") );
            destEnv.AddBinding("L",Term.ListTerm(ListHelper.BuildList<Valuable>(new Atom("b"),new Atom("c") )));

            var e1 = unifier.Eval(src, srcEnv);
            var e2 = unifier.Eval(dest, destEnv);
            Console.WriteLine($"SRC :>{e1} <");
            Console.WriteLine($"DEST :>{e2} <");
            
            var res = unifier.UnifyTerms(src, srcEnv, dest, new Unification());
            ;




        }
        
        static void Main(string[] args)
        {
//            PrologRepl.Loop(args);
Is();
//            AnonymiseTest();
            //RecursiveUnificationTest();
        }
    }
}