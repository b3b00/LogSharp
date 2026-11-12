using System.Collections.Generic;
using System.Linq;
using logSharp.interpretation;
using logSharp.interpretation.unification;
using logSharp.model;
using logSharp.parser;
using NFluent;
using sly.lexer;
using sly.parser;
using sly.parser.generator;
using Xunit;

namespace LogsharpTests
{
    public class BaseTest
    {
        protected Interpreter Interpreter;


        public BaseTest(string program)
        {
            var parser = BuildParser();
            var parsed = parser.Parse(program);
            Check.That(parsed).IsNotNull();
            Check.That(parsed.IsOk).IsTrue();
            Check.That(parsed.Result).IsNotNull();
            Check.That(parsed.Result).IsInstanceOf<LogSharpProgram>();
            
            
            ;
            Interpreter = new Interpreter(false, parsed.Result as LogSharpProgram, parser, new NestedTermUnifier());
            
        }
        private Parser<LogSharpToken, LogSharpAST> BuildParser()
        {
            var logParser = new LogSharpParser();
            var builder = new ParserBuilder<LogSharpToken, LogSharpAST>();
            var parser = builder.BuildParser(logParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "program");
            var lexer = parser.Result.Lexer as GenericLexer<LogSharpToken>;
            CallBacksBuilder.BuildCallbacks<LogSharpToken>(lexer);
            return parser.Result;
        }


        protected Question GetQuestion(string questionString)
        {
            var questionParse = Interpreter.Parser.Parse(questionString, "question");
            Assert.NotNull(questionParse);
            Assert.True(questionParse.IsOk);
            var question = questionParse.Result as Question;
            Assert.NotNull(question);
            return question;
        }

        protected List<Unification> GetAnswers(string questionString)
        {
            var question = GetQuestion(questionString);
            var answers = Interpreter.Search(question).ToList();
            return answers;
        }

    }
}