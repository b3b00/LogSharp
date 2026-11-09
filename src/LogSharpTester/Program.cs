using System;
using System.Collections.Generic;
using System.Linq;
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


        static void TestMemberOf()
        {
            string program = @"
member_of(X,[X|_]).
member_of(X,[_|L]) :- member_of(X,L).
";
            Interpreter = new Interpreter(true, null, BuildParser(), new NestedTermUnifier());
            Interpreter.DebugOutput = "c:/tmp/prolog/member.log";
            Interpreter.LoadProgram(program);
            // var answers = GetAnswers("member_of(a,[a,b,c])?");
           // PrintAnswers(answers);
           var answers = GetAnswers("member_of(c,[a,b,c])?");
           PrintAnswers(answers);
        }
        
        static void TestMapList()
        {
            string program = @"
mapper(a,aa).
mapper(b,bb).
mapper(c,cc).
";
            Interpreter = new Interpreter(true, null, BuildParser(), new NestedTermUnifier());
            Interpreter.Verbose = true;
            //Interpreter.DebugOutput = "c:/tmp/prolog/member.log";
            Interpreter.LoadProgram(program);
            // var answers = GetAnswers("member_of(a,[a,b,c])?");
            // PrintAnswers(answers);
            var answers = GetAnswers("map_list([a,b,c], mapper(X,Y), L)?");
            PrintAnswers(answers);
        }
        
        static void TestCall_1()
        {
            Console.WriteLine("************************************************");
            Console.WriteLine("***");
            Console.WriteLine("*** call/1");
            Console.WriteLine("***");
            Console.WriteLine("************************************************");
            string program = @"
callable.
";
            Interpreter = new Interpreter(true, null, BuildParser(), new NestedTermUnifier());
            Interpreter.Verbose = true;
            //Interpreter.DebugOutput = "c:/tmp/prolog/member.log";
            Interpreter.LoadProgram(program);
            // var answers = GetAnswers("member_of(a,[a,b,c])?");
            // PrintAnswers(answers);
            var answers = GetAnswers("call(callable)?");
            PrintAnswers(answers);
        }
        
        static void TestCall_2()
        {
            Console.WriteLine("************************************************");
            Console.WriteLine("***");
            Console.WriteLine("*** call/2");
            Console.WriteLine("***");
            Console.WriteLine("************************************************");
            string program = @"
single(a).
single(b).
";
            Interpreter = new Interpreter(true, null, BuildParser(), new NestedTermUnifier());
            Interpreter.Verbose = true;
            //Interpreter.DebugOutput = "c:/tmp/prolog/member.log";
            Interpreter.LoadProgram(program);
            // var answers = GetAnswers("member_of(a,[a,b,c])?");
            // PrintAnswers(answers);
            var answers = GetAnswers("call(single,X)?");
            PrintAnswers(answers);
        }
        
        static void TestCall_3()
        {
            //return;
            Console.WriteLine("************************************************");
            Console.WriteLine("***");
            Console.WriteLine("*** call/3");
            Console.WriteLine("***");
            Console.WriteLine("************************************************");
            string program = @"
relation(a,a).
relation(a,aa).
relation(a,aaa).
relation(b,bb).
relation(c,cc).
";
            Interpreter = new Interpreter(true, null, BuildParser(), new NestedTermUnifier());
            Interpreter.Verbose = false;
            //Interpreter.DebugOutput = "c:/tmp/prolog/member.log";
            Interpreter.LoadProgram(program);
            // var answers = GetAnswers("member_of(a,[a,b,c])?");
            // PrintAnswers(answers);
            var answers = GetAnswers("call(relation,X,Y)?");
            PrintAnswers(answers);
        }

        static void TestCof()
        {
            //return;
            Console.WriteLine("************************************************");
            Console.WriteLine("***");
            Console.WriteLine("*** call/3");
            Console.WriteLine("***");
            Console.WriteLine("************************************************");
            string program = @"
father(christian,olivier).
father(christian,frederic).
father(christian,julien).
father(frederic,lontzi).
father(frederic,eneko).
father(frederic,kepa).
father(julien,oihana).
grandpa(X,Z) :- father(X,Y) father(Y,Z).
gp(X,Y,Z) :- father(X,Y) father(Y,Z).
brother(X,Y) :- father(Z,X) father(Z,Y).

childrenOf(C,F) :- bag_of(CC,father(F,CC), L) member(C,L).
";
            Interpreter = new Interpreter(true, null, BuildParser(), new NestedTermUnifier());
            //Interpreter.DebugOutput = "c:/tmp/prolog/member.log";
            Interpreter.LoadProgram(program);
            
            // childrenOf(C,F) :- bag_of(CC,father(F,CC), L) member(C,L).
            // ️️⚠️ L variable (from bag_of()) does not get passed to member(C,L) /!\ ⚠️️️️ 
            // this is again a case of conjonction issue like with MathTest.TestComplexExpression
            
            var answers = GetAnswers("childrenOf(X,Y)?");
            
            if (answers.Count == 0 || !answers[0].IsOk)
            {
                Console.WriteLine("ERROR there are father with children");
            }
            else
            {
                
                foreach (var answer in answers)
                {
                    Console.WriteLine($"{answer.GetBinding("Y")} is the child of {answer.GetBinding("X")}");
                }
            }
            
            answers = GetAnswers("childrenOf(christian,olivier)?");
            if (answers.Count == 0 || !answers[0].IsOk)
            {
                Console.WriteLine("ERROR christian is really the father of olivier");
            }

            answers = GetAnswers("childrenOf(X,christian)?");
            
            if (answers.Count == 0 || !answers[0].IsOk)
            {
                Console.WriteLine("ERROR christian has children ! ");
            }
            else
            {
                var children = answers.Select(a => a.GetBinding("X").ToString()).ToList();
                foreach (var child in children)
                {
                    Console.WriteLine($"{child} is the child of christian");
                }
            }
            
            
        }

        static void TestCut()
        {
            string program = @"
            grade(Score, ""A"") :- Score >= 90 cut().
                grade(Score, ""B"") :- Score >= 75 cut().
                grade(Score, ""C"") :- Score >= 60 cut().
                grade(Score, ""F"") :- Score < 60.
";
            Interpreter = new Interpreter(false, null, BuildParser(), new NestedTermUnifier());
            Interpreter.LoadProgram(program);
            var answers = GetAnswers("grade(95,G)?");
            PrintAnswers(answers);
            answers = GetAnswers("grade(82,G)?");
            PrintAnswers(answers);
            answers = GetAnswers("grade(63,G)?");
            PrintAnswers(answers);  
            answers = GetAnswers("grade(6,G)?");
            PrintAnswers(answers);
        }
        
        static Interpreter Interpreter;
        
      


        static Question GetQuestion(string questionString)
        {
            var questionParse = Interpreter.Parser.Parse(questionString, "question");
            var question = questionParse.Result as Question;
            return question;
        }

        static List<Unification> GetAnswers(string questionString)
        {
            var question = GetQuestion(questionString);
            var answers = Interpreter.Search(question).ToList();
            return answers;
        }

        static void PrintAnswers(List<Unification> answers)
        {
            foreach (var answer in answers)
            {
                Console.WriteLine(answer);
            }
        }


        static LogSharpAST Parse(string source, string from = "scalarorTerm")
        {
            var parser = BuildParser();
            var xParsed = parser.Parse(source, from);
            if (xParsed.IsError)
            {
                throw new Exception(string.Join("\n", xParsed.Errors.Select(x => x.ErrorMessage)));
            }
            return xParsed.Result;
        }
        
        static void SortTwoWayBinding()
        {
            Console.WriteLine("************************************************");
            Console.WriteLine("***");
            Console.WriteLine("*** 2-way binding sort sort/2");
            Console.WriteLine("***");
            Console.WriteLine("************************************************");
            Interpreter = new Interpreter(true, null, BuildParser(), new NestedTermUnifier());
            Interpreter.Verbose = false;
            //Interpreter.DebugOutput = "c:/tmp/prolog/member.log";
            // var answers = GetAnswers("member_of(a,[a,b,c])?");
            // PrintAnswers(answers);
            var answers = GetAnswers("sort([a,b,c,d,e,f],list (_,list(X,T)))?");
            PrintAnswers(answers);
            answers = GetAnswers("sort([a,b,c,d,e,f],[_, X, T])?");
            PrintAnswers(answers);
            ;
        }

        static void TestMaths()
        {
        //return;
        Console.WriteLine("************************************************");
        Console.WriteLine("***");
        Console.WriteLine("*** call/3");
        Console.WriteLine("***");
        Console.WriteLine("************************************************");
        string program = @"
plusOne(X,Y) :- Y is X + 1.
conditionalAdd(X,Y) :- X < 10 Y is X + 1.
conditionalAdd(X,Y) :- X >= 10 Y is X + 10.
complex(X,Y) :- Y is (X + 69) * 12 - 4.
";
        Interpreter = new Interpreter(true, null, BuildParser(), new NestedTermUnifier());
        Interpreter.Verbose = true;
        Interpreter.DebugOutput = "c:/tmp/math.log";
        Interpreter.LoadProgram(program);
        // var answers = GetAnswers("member_of(a,[a,b,c])?");
        // PrintAnswers(answers);
        var answers = GetAnswers("conditionalAdd(42,Q43)?");
        PrintAnswers(answers);
        
        }
        
        static void Main(string[] args)
        {
            TestCut();
            //TestCof();
            //TestMaths();
            //PrologRepl.Loop(args);
            //TestMemberOf();
            // TestMapList();
            //SortTwoWayBinding();
            // TestCall_1();
            // TestCall_2();
            // TestCall_3();
//            AnonymiseTest();
            // RecursiveUnificationTest();
        }
    }
}