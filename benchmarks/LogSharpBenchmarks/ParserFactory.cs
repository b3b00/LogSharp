using sly.lexer;
using sly.parser;
using sly.parser.generator;
using logSharp.model;
using logSharp.parser;

namespace LogSharpBenchmarks;

public static class ParserFactory
{
    private static Parser<LogSharpToken, LogSharpAST> _cachedParser;

    public static Parser<LogSharpToken, LogSharpAST> GetParser()
    {
        if (_cachedParser == null)
        {
            var logParser = new LogSharpParser();
            var builder = new ParserBuilder<LogSharpToken, LogSharpAST>();
            var parser = builder.BuildParser(logParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "program");
            var lexer = parser.Result.Lexer as GenericLexer<LogSharpToken>;
            CallBacksBuilder.BuildCallbacks<LogSharpToken>(lexer);
            _cachedParser = parser.Result;
        }
        return _cachedParser;
    }
}
