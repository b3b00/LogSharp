using System.Linq;
using logSharp.model;
using logSharp.parser;
using sly.lexer;
using sly.parser;
using sly.parser.generator;
using Xunit;
using Xunit.Abstractions;

namespace ogsharpTests;

// temporary probe : DELETE ME
public class ScratchParseProbe
{
    private readonly ITestOutputHelper _output;

    public ScratchParseProbe(ITestOutputHelper output)
    {
        _output = output;
    }

    private (bool ok, string message) TryParse(string program)
    {
        var logParser = new LogSharpParser();
        var builder = new ParserBuilder<LogSharpToken, LogSharpAST>();
        var parser = builder.BuildParser(logParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "program");
        var lexer = parser.Result.Lexer as GenericLexer<LogSharpToken>;
        CallBacksBuilder.BuildCallbacks<LogSharpToken>(lexer);
        var parsed = parser.Result.Parse(program);
        if (parsed.IsOk) return (true, "");
        return (false, string.Join(" | ", parsed.Errors.Select(e => e.ErrorMessage)));
    }

    [Fact]
    public void ParsePieces()
    {
        _output.WriteLine("A: " + TryParse("h([X|Rest], X)."));
        _output.WriteLine("B: " + TryParse("third([_,_,Y|Rest], Y)."));
        _output.WriteLine("C: " + TryParse("p(X) :- length(L,3) h(L,X)."));
        _output.WriteLine("D: " + TryParse("r(A,B) :- length(L,3) two_elems(L,A,B)."));
        _output.WriteLine("E: " + TryParse("same_len(X,Y) :- length(X,N) length(Y,N)."));
        _output.WriteLine("F: " + TryParse("two_elems([A,B|Rest], A, B)."));
        _output.WriteLine("G: " + TryParse("callable_0()."));
        _output.WriteLine("H: " + TryParse("blocks([],[],[])."));
    }
}
