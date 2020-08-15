using System.Runtime.CompilerServices;
using logSharp.interpretation.unification;
using logSharp.model;
using logSharp.parser;
using sly.lexer;
using sly.parser;
using sly.parser.generator;
using Xunit;

namespace logsharpTests
{
    public class UnificationTests 
    {
        
        private Parser<LogSharpToken, ILogSharpAst> BuildParser()
        {
            var logParser = new LogSharpParser();
            var builder = new ParserBuilder<LogSharpToken, ILogSharpAst>();
            var parser = builder.BuildParser(logParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "program");
            var lexer = parser.Result.Lexer as GenericLexer<LogSharpToken>;
            CallBacksBuilder.BuildCallbacks<LogSharpToken>(lexer);
            return parser.Result;
        }


        public Term GetList(string list)
        {
            var parser = BuildParser();
            
            var r = parser.Parse(list,"list");
            Assert.True(r.IsOk);
            Assert.IsType<Term>(r.Result);
            return r.Result as Term;
            ;
        }

        [Fact]
        public void TestList()
        {
            var singleton = GetList("[a]");
            string dumpSingle = singleton.Dump();
            var trailer = GetList("[_|L]");
            string dumpTrailer = trailer.Dump();
            var unifier = new NestedTermUnifier();
            var sourceEnv = new Unification();
            var destEnv = new Unification();
            
            var unification = unifier.UnifyTerms(singleton, sourceEnv, trailer, destEnv);
            ;
        }
        
    }
}