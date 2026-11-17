using System;
using System.Linq;
using BenchmarkDotNet.Running;

namespace LogSharpBenchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        bool runQuick = args.Contains("--quick") || args.Length == 0 || !IsReleaseBuild();

        if (runQuick && !args.Contains("--benchmark"))
        {
            QuickBenchmarkRunner.Run();
            Console.WriteLine();
            Console.WriteLine("To run full statistical BenchmarkDotNet suites (Release mode):");
            Console.WriteLine("  dotnet run -c Release --project benchmarks/LogSharpBenchmarks -- --benchmark");
            return;
        }

        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args.Where(a => a != "--benchmark").ToArray());
    }

    private static bool IsReleaseBuild()
    {
#if DEBUG
        return false;
#else
        return true;
#endif
    }
}
