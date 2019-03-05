using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using logSharp.builtins.expressions;
using logSharp.builtins.expressions.model;
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
        private static Parser<ExpressionToken, Expression> expressionParser;
        
        public static Parser<ExpressionToken, Expression> ExpressionParser
        {
            get
            {
                if (expressionParser == null)
                {
                    var startingRule = $"{typeof(VariableExpressionParser).Name}_expressions";
                    var parserInstance = new VariableExpressionParser();
                    var builder = new ParserBuilder<ExpressionToken, Expression>();
                    expressionParser = builder.BuildParser(parserInstance, ParserType.LL_RECURSIVE_DESCENT, startingRule).Result;
                    ;
                }

                return expressionParser;
            }
        }
        
        public static Parser<LogSharpToken,ILogSharpAst> BuildParser()
        {
            var logParser = new LogSharpParser();
            var builder = new ParserBuilder<LogSharpToken, ILogSharpAst>();
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
        
        static void Assert()
        {
            string program = @"
            gt(X,Y) :- assert(""X > Y""). 
            lte(X,Y) :- assert(""X <= Y"").
            eq(X,Y) :- assert(""X == Y"").
            diff(X,Y) :- assert(""X != Y"").
";
            var parser = BuildParser();
            var parsed = parser.Parse(program);
            
            List<string> questions = ListHelper.BuildList("gt(1, 2)?","lte(2 , 2)?","eq(3,3)?","diff(48,12)");

            foreach (var q in questions)
            {
                
                Interpreter interpreter = new Interpreter(false,parsed.Result as LogSharpProgram, parser, new NestedTermUnifier());
                
                var r = interpreter.Search(q).ToList();
                Console.WriteLine(q);
                r.ForEach(u => Console.WriteLine(u.ToString())); 
                ; 
            }
            
            if (parsed.IsOk)
            {
                Interpreter interpreter = new Interpreter(false,parsed.Result as LogSharpProgram, parser, new NestedTermUnifier());
                
                var r = interpreter.Search("get(1,2)?").ToList();
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
            srcEnv.AddBinding("L",Term.ListTerm(ListHelper.BuildList<IValuable>(new Atom("b"),new Atom("c") )));

            
            var destEnv = new Unification();
            var emptyChildU = new Unification();
            
            destEnv.AddBinding("X",new Atom("c") );
            destEnv.AddBinding("L",Term.ListTerm(ListHelper.BuildList<IValuable>(new Atom("b"),new Atom("c") )));

            var e1 = unifier.Eval(src, srcEnv);
            var e2 = unifier.Eval(dest, destEnv);
            Console.WriteLine($"SRC :>{e1} <");
            Console.WriteLine($"DEST :>{e2} <");
            
            var res = unifier.UnifyTerms(src, srcEnv, dest, new Unification());
            ;




        }
        
        static void Main(string[] args)
        {
//            var r = ExpressionParser.Parse("X < Y");
//            ;
//            PrologRepl.Loop(args);
//Is();
            Assert();
//            AnonymiseTest();
            //RecursiveUnificationTest();
        }
    }
}