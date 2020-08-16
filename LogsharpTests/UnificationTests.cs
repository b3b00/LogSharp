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


        public Term Get(string list, string start)
        {
            var parser = BuildParser();
            
            var r = parser.Parse(list,start);
            Assert.True(r.IsOk);
            Assert.IsType<Term>(r.Result);
            return r.Result as Term;
            ;
        }
        
        public Term GetList(string list)
        {
            return Get(list, "list");
        }

        public Term GetTerm(string term)
        {
            return Get(term, "term");
        }

        [Fact]
        public void TestNested()
        {
            var singleton = GetTerm("f(a,f(b,c))");
            string dumpSingle = singleton.Dump();
            var trailer = GetTerm("f(X,Y)");
            string dumpTrailer = trailer.Dump();
            var unifier = new NestedTermUnifier();
            var sourceEnv = new Unification();
            var destEnv = new Unification();
            
            var unification = unifier.UnifyTerms(singleton, sourceEnv, trailer, destEnv);
            Assert.True(unification.IsOk);
            Assert.True(unification.Environment.ContainsVariable("X"));
            var x = unification.Environment["X"];
            Assert.NotNull(x);
            Assert.IsType<Atom>(x);
            Assert.Equal("a",(x as Atom)?.Name);
            Assert.True(unification.Environment.ContainsVariable("Y"));
            var y = unification.Environment["Y"];
            Assert.NotNull(y);
            Assert.IsType<Term>(y);
            var yt = y as Term;
            Assert.Equal("f/2",yt.Specification);
            Assert.IsType<Atom>(yt.Parameters[0]);
            Assert.Equal("b", (yt.Parameters[0] as Atom)?.Name);
            Assert.IsType<Atom>(yt.Parameters[1]);
            Assert.Equal("c", (yt.Parameters[1] as Atom)?.Name);
        }
        
        [Fact]
        public void TestSubNested()
        {
            var singleton = GetTerm("f(a,f(b,c))");
            string dumpSingle = singleton.Dump();
            var trailer = GetTerm("f(X,f(b,Y))");
            string dumpTrailer = trailer.Dump();
            var unifier = new NestedTermUnifier();
            var sourceEnv = new Unification();
            var destEnv = new Unification();
            
            var unification = unifier.UnifyTerms(singleton, sourceEnv, trailer, destEnv);
            Assert.True(unification.IsOk);
            Assert.True(unification.Environment.ContainsVariable("X"));
            var x = unification.Environment["X"];
            Assert.NotNull(x);
            Assert.IsType<Atom>(x);
            Assert.Equal("a",(x as Atom)?.Name);
            Assert.True(unification.Environment.ContainsVariable("Y"));
            var y = unification.Environment["Y"];
            Assert.NotNull(y);
            Assert.IsType<Atom>(y);
            Assert.Equal("c",(y as Atom)?.Name);
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