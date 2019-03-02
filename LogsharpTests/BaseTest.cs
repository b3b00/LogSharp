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

namespace ogsharpTests
{
    public class BaseTest
    {
        protected Interpreter Interpreter;


        public BaseTest(string program)
        {
            var parser = BuildParser();
            Interpreter = new Interpreter(false, null, parser, new NestedTermUnifier());
            Interpreter.Verbose = true;
            Interpreter.LoadProgram(program);
        }

        private Parser<LogSharpToken, LogSharpAST> BuildParser()
        {
            var logParser = new LogSharpParser();
            var builder = new ParserBuilder<LogSharpToken, LogSharpAST>();
            var parser = builder.BuildParser(logParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "program");
            var lexer = parser.Result.Lexer as GenericLexer<LogSharpToken>;
            CallBacksBuilder.BuildCallbacks(lexer);
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