using System;
using System.Collections.Generic;
using System.Linq;
using logSharp.model;
using logSharp.helpers;
using logSharp.interpretation;
using logSharp.interpretation.unification;
using logSharp.parser;
using sly.lexer;
using sly.parser;
using sly.parser.generator;

namespace LogSharpTester
{
    class Program
    {
        
        
        public static Parser<LogSharpToken,ILogSharpAst> BuildParser()
        {
            var logParser = new LogSharpParser();
            var builder = new ParserBuilder<LogSharpToken, ILogSharpAst>();
            var parser = builder.BuildParser(logParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "program");
            var lexer = parser.Result.Lexer as GenericLexer<LogSharpToken>;
            CallBacksBuilder.BuildCallbacks<LogSharpToken>(lexer);
            return parser.Result;
        }


        static void Members()
        {
            string program = @"
member_of(X,[X]).
member_of(X,[X|_]).
member_of(X,[_|L]) :- member_of(X,L).";
         
         
            var parser = BuildParser();
            var parsed = parser.Parse(program);
            if (parsed.IsOk)
            {
                Interpreter interpreter = new Interpreter(false,parsed.Result as LogSharpProgram, parser, new NestedTermUnifier());
                interpreter.Verbose = true;
                var r = interpreter.Search("member_of(c,[c])?").ToList();
                if (r != null)
                {
                    int i = 0;
                    foreach (var unification in r)
                    {
                        Console.WriteLine($"unification #{i}");
                        if (unification.IsOk)
                        {
                            Console.WriteLine("\tOK");
                            Console.WriteLine($"\t{unification}");
                        }
                        else
                        {
                            Console.WriteLine("\tKO");
                        }
                    }
                }
                r.ForEach(u => Console.WriteLine(u.ToString())); 
                ;
            }
         
        }

        static void NestedQuestion()
        {

//             string PrologProgram = @"
// is_in(X,l(X,_)).
// is_in(X,l(_,TAIL)) :- is_in(X,TAIL).";
//             //string questionString = "is_in(K,l(a,l(b,l(c,l(d,l(e,()))))))?";
//             string questionString = "is_in(c,l(a,l(b,l(c,()))))?";

            string PrologProgram = @"
member_of(X,[X|_]).
member_of(X,[_|L]) :- member_of(X,L).";

            string questionString = "member_of(c,[a,b,c])?";
            //questionString = "member_of(c, [b|[c|[]]])?";
        


            var parser = BuildParser();
            var parsed = parser.Parse(PrologProgram);

            // var prog = parsed.Result as LogSharpProgram;
            // int i = 0;
            // foreach (var rule in prog.Rules)
            // {
            //     Console.WriteLine(rule.Dump());
            //     foreach (var free in rule.FreeVariables)
            //     {
            //         if (!free.IsDiscardableVariable)
            //         {
            //             free.Name = free.Name + "_" + i;
            //         }
            //     }
            //     Console.WriteLine(rule.Dump());
            //     i++;
            // }
            
            if (parsed.IsOk)
            {
                Interpreter interpreter = new Interpreter(false, parsed.Result as LogSharpProgram, parser,
                    new NestedTermUnifier());
                interpreter.VeryVerbose = true;

                var questionParse = parser.Parse(questionString, "question");
                var question = questionParse.Result as Question;
                

                interpreter.Verbose = true;
                var answers = interpreter.Search(question).ToList();
                ;

                if (answers != null && answers.Any() && answers.First().IsOk)
                {
                    Console.WriteLine("OK");
                    foreach (var answer in answers)
                    {
                        Console.WriteLine(answer);
                    }
                }
                else
                {
                    Console.WriteLine("KO");
                }
            }
        }

        static void Is()
        {
            string program = @"
plusone(X,Y) :- X is Y + 1 .
cstexpr(X) :- X is 1 + 1 . 
complexexpr(R,X,Y,Z) :- R is X + Y * Z).
";
            var parser = BuildParser();
            var parsed = parser.Parse(program);
            var t = parser.Parse(@"complexexpr(A,100,2,6)?","question");
            if (parsed.IsOk)
            {
                Interpreter interpreter = new Interpreter(false,parsed.Result as LogSharpProgram, parser, new NestedTermUnifier());
                
                var r = interpreter.Search("plusone(K,41)?").ToList();
                r.ForEach(u => Console.WriteLine(u.ToString())); 
                ;
            }



        }
        
        static void Assert()
        {
            string program = @"
            gt(X,Y) :- assert(X > Y). 
            lte(X,Y) :- assert(X <= Y).
            eq(X,Y) :- assert(X == Y).
            diff(X,Y) :- assert(X != Y).
            gtc(X,Y) :- assert(X + Y > X * Y).";
            program = @"
            gt(X,Y) :- X > Y. 
            lte(X,Y) :- X <= Y.
            eq(X,Y) :- X == Y.
            diff(X,Y) :- X != Y.
            gtc(X,Y) :- X + Y > X * Y.";
            var parser = BuildParser();
            var parsed = parser.Parse(program);
            
            List<string> questions = ListHelper.BuildList(
                "gt(1, 2)?",
                "lte(2 , 2)?",
                "eq(3,3)?",
                "diff(48,12)?",
                "eq(48,12)?",
                "gtc(2,1)?");

            foreach (var q in questions)
            {
                Console.WriteLine(q);
                Interpreter interpreter = new Interpreter(false,parsed.Result as LogSharpProgram, parser, new NestedTermUnifier());
                
                var r = interpreter.Search(q).ToList();
                
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
            srcEnv.AddBinding("L",Term.LiteralListTerm(ListHelper.BuildList<IValuable>(new Atom("b"),new Atom("c") )));

            
            var destEnv = new Unification();
            var emptyChildU = new Unification();
            
            destEnv.AddBinding("X",new Atom("c") );
            destEnv.AddBinding("L",Term.LiteralListTerm(ListHelper.BuildList<IValuable>(new Atom("b"),new Atom("c") )));

            var e1 = unifier.Eval(src, srcEnv);
            var e2 = unifier.Eval(dest, destEnv);
            Console.WriteLine($"SRC :>{e1} <");
            Console.WriteLine($"DEST :>{e2} <");
            
            var res = unifier.UnifyTerms(src, srcEnv, dest, new Unification());
            ;

        }

        static void ChildOf()
        {
            
            var parser = BuildParser();
            
           
            
           
                Interpreter interpreter = new Interpreter(false,null, parser, new NestedTermUnifier());

                interpreter.LoadProgramFile("f.pro");

                interpreter.Verbose = true;
                interpreter.DebugOutput = "log.log";
                var unifications = interpreter.Search("cof(olivier,christian)?").ToList();

                foreach (var u in unifications)
                {
                    Console.WriteLine(u);
                }
                ;

        } 
        
        static void Main(string[] args)
        {
//            var r = ExpressionParser.Parse("X < Y");
//            ;
//            PrologRepl.Loop(args);
//Is();
          //Assert();
          //ChildOf();
//            AnonymiseTest();
            //RecursiveUnificationTest();
            //Members();
            NestedQuestion();
        }
    }
}