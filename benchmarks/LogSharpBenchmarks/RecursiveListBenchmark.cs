using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using logSharp.interpretation;
using logSharp.interpretation.unification;
using logSharp.model;

namespace LogSharpBenchmarks;

[MemoryDiagnoser]
public class RecursiveListBenchmark
{
    private Interpreter _interpIndexed;
    private Interpreter _interpLinear;
    private Question _queryHead;
    private Question _queryFail;

    [GlobalSetup]
    public void Setup()
    {
        string program = @"
item_at(X, [X | _]).
item_at(X, [_ | Tail]) :- item_at(X, Tail).
";
        var parser = ParserFactory.GetParser();

        var parsedIndexed = parser.Parse(program);
        var progIndexed = parsedIndexed.Result as LogSharpProgram;
        progIndexed.UseIndex = true;
        _interpIndexed = new Interpreter(false, progIndexed, parser, new NestedTermUnifier());

        var parsedLinear = parser.Parse(program);
        var progLinear = parsedLinear.Result as LogSharpProgram;
        progLinear.UseIndex = false;
        _interpLinear = new Interpreter(false, progLinear, parser, new NestedTermUnifier());

        // Build a 100-element list
        var elements = Enumerable.Range(0, 100).Select(i => (Valuable)new Int(i)).ToList();
        var listTerm = Term.ListTerm(elements);

        // Query matching list head: item_at(X, [0, 1, ...])
        _queryHead = new Question(new Term("item_at", new List<Valuable>
        {
            new Variable("X"),
            listTerm
        }));

        // Query matching empty list (fails immediately)
        _queryFail = new Question(new Term("item_at", new List<Valuable>
        {
            new Int(999),
            Term.EmptyListTerm()
        }));
    }

    [Benchmark(Baseline = true)]
    public List<Unification> Linear_QueryFail()
    {
        return _interpLinear.Search(_queryFail).ToList();
    }

    [Benchmark]
    public List<Unification> Indexed_QueryFail()
    {
        return _interpIndexed.Search(_queryFail).ToList();
    }
}
