using System;
using LogSharp.model;
using LogSharp.parser;
using sly.buildresult;
using sly.parser;
using sly.parser.generator;
using System.Linq;

namespace LogSharp
{
    class Program
    {

        static BuildResult<Parser<LogSharpToken,LogSharpAST>> BuildParser()
        {
            var logParser = new LogSharpParser();
            var builder = new ParserBuilder<LogSharpToken, LogSharpAST>();
            var parser = builder.BuildParser(logParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "program");
            return parser;
        }
        
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var parser = BuildParser();
            if (parser.IsError)
            {
                ;
                parser.Errors.ForEach(e => Console.WriteLine($"ERROR : {e.Level}  - {e.Message}"));
            }
            else
            {
                Console.WriteLine(("OK !"));
            }
        }
    }
}