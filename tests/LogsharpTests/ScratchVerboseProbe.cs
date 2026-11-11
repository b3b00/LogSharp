using System.Collections.Generic;
using System.IO;
using System.Linq;
using logSharp.interpretation;
using logSharp.interpretation.unification;
using logSharp.model;
using logSharp.parser;
using sly.lexer;
using sly.parser;
using sly.parser.generator;
using Xunit;
using Xunit.Abstractions;

namespace ogsharpTests;

// temporary probe : DELETE ME
public class ScratchVerboseProbe
{
    private readonly ITestOutputHelper _output;

    public ScratchVerboseProbe(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ProbeVerbose()
    {
        var program = @"
h([X|Rest], X).
p(X) :- length(L,3) h(L,X).
";
        var logParser = new LogSharpParser();
        var builder = new ParserBuilder<LogSharpToken, LogSharpAST>();
        var parser = builder.BuildParser(logParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "program");
        var lexer = parser.Result.Lexer as GenericLexer<LogSharpToken>;
        CallBacksBuilder.BuildCallbacks<LogSharpToken>(lexer);
        var parsed = parser.Result.Parse(program);
        Assert.True(parsed.IsOk, parsed.IsOk ? "" : string.Join("|", (parsed.Errors ?? new List<ParseError>()).Select(e => e.ErrorMessage)));
        var logPath = Path.Combine(Path.GetTempPath(), "opencode", "verbose_trace.log");
        File.WriteAllText(logPath, "");
        var interpreter = new Interpreter(true, parsed.Result as logSharp.model.LogSharpProgram, parser.Result,
            new NestedTermUnifier())
        {
            DebugOutput = logPath
        };
        var question = interpreter.Parser.Parse("p(X)?", "question").Result as Question;
        try
        {
            var results = interpreter.Search(question);
            foreach (var r in results)
            {
                _output.WriteLine("result: " + r);
            }
        }
        catch (System.Exception e)
        {
            _output.WriteLine("EXCEPTION: " + e.GetType().Name);
        }

        var lines = File.ReadAllLines(logPath);
        _output.WriteLine("total trace lines: " + lines.Length);
        foreach (var line in lines[^40..])
        {
            _output.WriteLine(line);
        }
    }
}
