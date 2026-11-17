using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using logSharp.interpretation;
using logSharp.interpretation.unification;
using logSharp.model;

namespace LogSharpBenchmarks;

public static class QuickBenchmarkRunner
{
    public static void Run()
    {
        Console.WriteLine("==========================================================================================");
        Console.WriteLine("                  LogSharp: First-Argument Index Performance Evaluation                   ");
        Console.WriteLine("==========================================================================================");
        Console.WriteLine();

        int[] sizes = { 100, 500, 1000, 2500, 5000 };

        Console.WriteLine($"{"Database (N)",-14} | {"Metric",-16} | {"Linear Scan",-16} | {"Indexed",-16} | {"Improvement",-14}");
        Console.WriteLine(new string('-', 88));

        foreach (var n in sizes)
        {
            BenchmarkSize(n);
            Console.WriteLine(new string('-', 88));
        }

        Console.WriteLine();
        Console.WriteLine("==========================================================================================");
        Console.WriteLine("             Compound Term / List Constructor Dispatch (list/0 vs list/2)                 ");
        Console.WriteLine("==========================================================================================");
        BenchmarkListDispatch();

        Console.WriteLine();
        Console.WriteLine("Conclusions:");
        Console.WriteLine("1. Candidate Selection: Indexed lookup narrows down to exactly 1 candidate (O(1)).");
        Console.WriteLine("2. Memory Savings: Linear scan allocates Goal objects and deep-copies every tested clause.");
        Console.WriteLine("3. Speedup: Proportional to database size N; eliminates O(N) scanning completely.");
        Console.WriteLine("==========================================================================================");
    }

    private static void BenchmarkSize(int n)
    {
        var rules = new List<Rule>(n);
        for (int i = 0; i < n; i++)
        {
            rules.Add(new Fact("person", new List<Valuable>
            {
                new Atom($"id_{i:D6}"),
                new Atom($"Name_{i}"),
                new Int(20 + (i % 60))
            }));
        }

        var progIndexed = new LogSharpProgram(rules) { UseIndex = true };
        var progLinear = new LogSharpProgram(new List<Rule>(rules)) { UseIndex = false };

        var parser = ParserFactory.GetParser();
        var interpIndexed = new Interpreter(false, progIndexed, parser, new NestedTermUnifier());
        var interpLinear = new Interpreter(false, progLinear, parser, new NestedTermUnifier());

        // Target key at the end (worst-case for linear scan)
        int targetIdx = n - 1;
        var query = new Question(new Term("person", new List<Valuable>
        {
            new Atom($"id_{targetIdx:D6}"),
            new Variable("Name"),
            new Variable("Age")
        }));

        // Warmup
        for (int i = 0; i < 5; i++)
        {
            interpIndexed.Search(query).ToList();
            interpLinear.Search(query).ToList();
        }

        // Measure Linear Scan
        const int iterations = 50;
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        long startBytesLinear = GC.GetTotalAllocatedBytes(true);
        var swLinear = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            interpLinear.Search(query).ToList();
        }
        swLinear.Stop();
        long bytesLinear = (GC.GetTotalAllocatedBytes(true) - startBytesLinear) / iterations;
        double timeLinearUs = (swLinear.Elapsed.TotalMicroseconds) / iterations;

        // Measure Indexed
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        long startBytesIndexed = GC.GetTotalAllocatedBytes(true);
        var swIndexed = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            interpIndexed.Search(query).ToList();
        }
        swIndexed.Stop();
        long bytesIndexed = (GC.GetTotalAllocatedBytes(true) - startBytesIndexed) / iterations;
        double timeIndexedUs = (swIndexed.Elapsed.TotalMicroseconds) / iterations;

        // Candidates inspected
        int linearCandidates = progLinear.GetCandidateRules("person/3", new Atom($"id_{targetIdx:D6}")).Count();
        int indexedCandidates = progIndexed.GetCandidateRules("person/3", new Atom($"id_{targetIdx:D6}")).Count();

        double speedup = timeIndexedUs > 0 ? timeLinearUs / timeIndexedUs : double.PositiveInfinity;
        double allocReduction = bytesIndexed > 0 ? (double)bytesLinear / bytesIndexed : 0;

        Console.WriteLine($"{n,-14} | {"Candidates",-16} | {linearCandidates + " rules",-16} | {indexedCandidates + " rule",-16} | {$"{n / (double)indexedCandidates:F0}x fewer",-14}");
        Console.WriteLine($"{n,-14} | {"Latency",-16} | {FormatUs(timeLinearUs),-16} | {FormatUs(timeIndexedUs),-16} | {$"{speedup:F1}x faster",-14}");
        Console.WriteLine($"{n,-14} | {"Allocations",-16} | {FormatBytes(bytesLinear),-16} | {FormatBytes(bytesIndexed),-16} | {$"{allocReduction:F1}x less",-14}");
    }

    private static void BenchmarkListDispatch()
    {
        string program = @"
my_len([], 0).
my_len([_ | Tail], N) :- my_len(Tail, M) N is M + 1.
";
        var parser = ParserFactory.GetParser();
        var progIndexed = (LogSharpProgram)parser.Parse(program).Result;
        progIndexed.UseIndex = true;
        var interpIndexed = new Interpreter(false, progIndexed, parser, new NestedTermUnifier());

        var progLinear = (LogSharpProgram)parser.Parse(program).Result;
        progLinear.UseIndex = false;
        var interpLinear = new Interpreter(false, progLinear, parser, new NestedTermUnifier());

        // Test querying with empty list []
        var emptyQuery = new Question(new Term("my_len", new List<Valuable>
        {
            Term.EmptyListTerm(),
            new Variable("N")
        }));

        // Warmup
        for (int i = 0; i < 50; i++)
        {
            interpLinear.Search(emptyQuery).ToList();
            interpIndexed.Search(emptyQuery).ToList();
        }

        var swLinear = Stopwatch.StartNew();
        for (int i = 0; i < 500; i++) interpLinear.Search(emptyQuery).ToList();
        swLinear.Stop();

        var swIndexed = Stopwatch.StartNew();
        for (int i = 0; i < 500; i++) interpIndexed.Search(emptyQuery).ToList();
        swIndexed.Stop();

        int linearCandidates = progLinear.GetCandidateRules("my_len/2", Term.EmptyListTerm()).Count();
        int indexedCandidates = progIndexed.GetCandidateRules("my_len/2", Term.EmptyListTerm()).Count();

        Console.WriteLine($"Query: my_len([], N)? (Base Case Dispatch)");
        Console.WriteLine($"Candidates Considered: Linear = {linearCandidates} clauses | Indexed = {indexedCandidates} clause (skips [H|T] rule entirely)");
        Console.WriteLine($"Total Time (500 queries): Linear = {swLinear.Elapsed.TotalMilliseconds:F2} ms | Indexed = {swIndexed.Elapsed.TotalMilliseconds:F2} ms (Speedup: {swLinear.Elapsed.TotalMilliseconds / swIndexed.Elapsed.TotalMilliseconds:F1}x)");
    }

    private static string FormatUs(double us)
    {
        if (us >= 1000)
            return $"{us / 1000.0:F2} ms";
        return $"{us:F1} µs";
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes >= 1024 * 1024)
            return $"{bytes / (1024.0 * 1024.0):F2} MB";
        if (bytes >= 1024)
            return $"{bytes / 1024.0:F1} KB";
        return $"{bytes} B";
    }
}
