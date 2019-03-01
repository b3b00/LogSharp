using System.Collections.Generic;
using System.Linq;
using logSharp.interpretation;
using logSharp.interpretation.unification;
using logSharp.model;
using logSharp.parser;
using sly.lexer;
using sly.parser;
using sly.parser.generator;
using Xunit;
using Xunit.Sdk;

namespace ogsharpTests
{
    public class FamilyTests
    {
        private Interpreter Interpreter;

        public  const string PrologProgram = @"
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

myl([a,b,c]).
myl([d,e,f]).

tester(X,Y) :- testPrim(X,Y).

member_of(X,[X|_]).
member_of(X,[_|L]) :- member_of(X,L).

cof(C,F) :- bag_of(CC,""father(F,CC)"", L) member(C,L).";

        
        

        public FamilyTests()
        {
            var parser = BuildParser();
            Interpreter = new Interpreter(false, null, parser, new NestedTermUnifier());
            Interpreter.LoadProgram(PrologProgram);
        }
        
        
        private Parser<LogSharpToken,LogSharpAST> BuildParser()
        {
            var logParser = new LogSharpParser();
            var builder = new ParserBuilder<LogSharpToken, LogSharpAST>();
            var parser = builder.BuildParser(logParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "program");
            var lexer = parser.Result.Lexer as GenericLexer<LogSharpToken>;
            CallBacksBuilder.BuildCallbacks<LogSharpToken>(lexer);
            return parser.Result;
        }


        private Question GetQuestion(string questionString)
        {
            var questionParse = Interpreter.Parser.Parse(questionString,"question");
            Assert.NotNull(questionParse);
            Assert.True(questionParse.IsOk);
            var question = questionParse.Result as Question;
            Assert.NotNull(question);
            return question;
        }
        
        [Fact]
        public void FathersTest()
        {
            var question = GetQuestion("father(X,Y)?");
            var answers = Interpreter.Search(question).ToList();
            Assert.NotNull(answers);
            Assert.Equal(7,answers.Count);
            var c = answers.Where(a =>
                    a.IsOk && a.Environment.ContainsVariable("X") && a.Environment["X"]?.ToString() == "christian")
                .ToList();
            Assert.Equal(3,c.Count);
            var f = answers.Where(a =>
                    a.IsOk && a.Environment.ContainsVariable("X") && a.Environment["X"]?.ToString() == "frederic")
                .ToList();
            Assert.Equal(3,f.Count);
            var j = answers.Where(a =>
                    a.IsOk && a.Environment.ContainsVariable("X") && a.Environment["X"]?.ToString() == "julien")
                .ToList();
            Assert.Single(j);
        }

        [Fact]
        public void GrandPaTest()
        {
            var question = GetQuestion("grandpa(christian,oihana)?");
            var answers = Interpreter.Search(question).ToList();
            Assert.Single(answers);
            Assert.True(answers[0].IsOk);
            Assert.Empty(answers[0].Environment.Environment);
            
            question = GetQuestion("grandpa(christian,X)?");
            answers = Interpreter.Search(question).ToList();
            Assert.NotNull(answers);
            Assert.Equal(4,answers.Count);
            var grandchild = answers.Select(a => a.Environment["X"].ToString()).ToList();
            var shouldList = new List<string>() {"lontzi","eneko","kepa","oihana"};
            foreach (var shouldItem in shouldList)
            {
                Assert.Contains(grandchild, x => x == shouldItem);
            }

        }

        [Fact]
        public void ChildOfTest()
        {
            var question = GetQuestion("cof(christian,olivier)?");
            var answers = Interpreter.Search(question).ToList();
            Assert.NotNull(answers);
            Assert.True(answers[0].IsOk);
            Assert.Empty(answers[0].Environment.Environment);
            
            question = GetQuestion("cof(christian,X)?");
            answers = Interpreter.Search(question).ToList();
            Assert.NotNull(answers);
            Assert.Equal(3,answers.Count);
            var children = answers.Select(a => a.Environment["X"].ToString()).ToList();
            var shouldList = new List<string>() {"frederic","olivier","julien"};
            foreach (var shouldItem in shouldList)
            {
                Assert.Contains(children, x => x == shouldItem);
            }
        }
        
        
    }
}