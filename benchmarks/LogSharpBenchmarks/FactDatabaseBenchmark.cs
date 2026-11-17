using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using logSharp.interpretation;
using logSharp.interpretation.unification;
using logSharp.model;

namespace LogSharpBenchmarks;

[MemoryDiagnoser]
public class FactDatabaseBenchmark
{
    private Interpreter _interpreterIndexed;
    private Interpreter _interpreterLinear;
    private Question _queryMiddle;
    private Question _queryEnd;
    private Question _queryMiss;
    private Question _queryUnbound;

    [Params(100, 1000, 5000)]
    public int DatabaseSize { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var rules = new List<Rule>(DatabaseSize);
        for (int i = 0; i < DatabaseSize; i++)
        {
            rules.Add(new Fact("record", new List<Valuable>
            {
                new Atom($"key_{i:D6}"),
                new Int(i * 10)
            }));
        }

        var progIndexed = new LogSharpProgram(rules) { UseIndex = true };
        var progLinear = new LogSharpProgram(new List<Rule>(rules)) { UseIndex = false };

        var parser = ParserFactory.GetParser();
        _interpreterIndexed = new Interpreter(false, progIndexed, parser, new NestedTermUnifier());
        _interpreterLinear = new Interpreter(false, progLinear, parser, new NestedTermUnifier());

        int midIndex = DatabaseSize / 2;
        int endIndex = DatabaseSize - 1;

        _queryMiddle = new Question(new Term("record", new List<Valuable>
        {
            new Atom($"key_{midIndex:D6}"),
            new Variable("Val")
        }));

        _queryEnd = new Question(new Term("record", new List<Valuable>
        {
            new Atom($"key_{endIndex:D6}"),
            new Variable("Val")
        }));

        _queryMiss = new Question(new Term("record", new List<Valuable>
        {
            new Atom("non_existent_key"),
            new Variable("Val")
        }));

        _queryUnbound = new Question(new Term("record", new List<Valuable>
        {
            new Variable("K"),
            new Variable("Val")
        }));
    }

    [Benchmark(Baseline = true)]
    public List<Unification> LinearScan_QueryMiddle()
    {
        return _interpreterLinear.Search(_queryMiddle).ToList();
    }

    [Benchmark]
    public List<Unification> Indexed_QueryMiddle()
    {
        return _interpreterIndexed.Search(_queryMiddle).ToList();
    }

    [Benchmark]
    public List<Unification> LinearScan_QueryEnd()
    {
        return _interpreterLinear.Search(_queryEnd).ToList();
    }

    [Benchmark]
    public List<Unification> Indexed_QueryEnd()
    {
        return _interpreterIndexed.Search(_queryEnd).ToList();
    }

    [Benchmark]
    public List<Unification> LinearScan_QueryMiss()
    {
        return _interpreterLinear.Search(_queryMiss).ToList();
    }

    [Benchmark]
    public List<Unification> Indexed_QueryMiss()
    {
        return _interpreterIndexed.Search(_queryMiss).ToList();
    }
}
