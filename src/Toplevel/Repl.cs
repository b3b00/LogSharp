using System;
using System.Collections.Generic;
using System.Linq;
using sly.parser;
using sly.parser.generator;
using logSharp.interpretation;
using logSharp.interpretation.unification;
using logSharp.model;
using logSharp.parser;
using sly.lexer;
using Utility.CommandLine;

namespace Toplevel;



public class PrologRepl
{

    [Argument('v', "verbose", "verbosity")]
    private static string LogFile { get; set; }

    delegate void ReplAction(Interpreter interpreter, Question question);

    private static Dictionary<string, ReplAction> ReplActions = new Dictionary<string, ReplAction>()
    {
        { "load", Load },
        { "trace", Trace },
        { "quiet", Quiet },
        { "dump", Dump }
    };

    static Parser<LogSharpToken, LogSharpAST> BuildParser()
    {
        var logParser = new LogSharpParser();
        var builder = new ParserBuilder<LogSharpToken, LogSharpAST>();
        var parser = builder.BuildParser(logParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "program");
        if (parser.IsError)
        {
            Console.WriteLine("error building prolog parser");
            foreach (var error in parser.Errors)
            {
                Console.WriteLine(error.Message);
            }
            Environment.Exit(1);
        }
        var lexer = parser.Result.Lexer as GenericLexer<LogSharpToken>;
        CallBacksBuilder.BuildCallbacks<LogSharpToken>(lexer);
        return parser.Result;
    }


    static void Load(Interpreter interpreter, Question ask)
    {
        if (ask.Parameters.Count == 1)
        {
            var param = ask.Parameters[0];
            if (param is Atom atom)
            {
                interpreter.LoadProgramFile(atom.Name);
            }
            else if (param is logSharp.model.String str)
            {
                interpreter.LoadProgramFile(str.Value);
            }
            else
            {
                Console.Error.WriteLine(" bad parameter");
            }
        }
        else
        {
            Console.Error.WriteLine("bad query. load takes 1 parameter.");
        }
    }


    static void Trace(Interpreter interpreter, Question ask)
    {
        if (ask.Parameters.Count <= 1)
        {
            string outFile = null;
            if (ask.Parameters.Count == 1)
            {
                var param = ask.Parameters[0];
                if (param is Atom atom)
                {
                    outFile = atom.Name;
                }
                else if (param is logSharp.model.String str)
                {
                    outFile = str.Value;
                }
                else
                {
                    Console.Error.WriteLine(" bad parameter");
                }
            }

            interpreter.DebugOutput = outFile;
            interpreter.Verbose = true;
        }
        else
        {
            Console.Error.WriteLine("bad query. trace takes 0 or 1 parameter.");
        }
    }


    static void Quiet(Interpreter interpreter, Question ask)
    {
        interpreter.Verbose = false;
    }

    static void Dump(Interpreter interpreter, Question ask)
    {
        if (ask.Parameters.Count == 0 || (ask.Parameters.Count == 1 &&  ask.Parameters[0] is Atom rulesAtom && (rulesAtom.Name == "all" || rulesAtom.Name=="rules"))) {
            interpreter.LogSharpProgram.Rules.ForEach(r => Console.WriteLine(r));
        }

        if (ask.Parameters.Count == 0 || (ask.Parameters.Count == 1 && ask.Parameters[0] is Atom builtinAtoms &&
                                          (builtinAtoms.Name == "all" || builtinAtoms.Name == "builtin" || builtinAtoms.Name == "prim" || builtinAtoms.Name == "primitives")))
        {
            interpreter.BuiltIns.Keys.ToList().ForEach(b => Console.WriteLine(b));
        }
    }


    public static void Loop(string[] args)
    {
        Arguments.Populate();
        var parser = BuildParser();
        Console.WriteLine($"startiing prolog with {(args != null && args.Length > 0 ? string.Join(" ",args):" no arg")}");

        LogSharpProgram program = null;
        if (args.Length > 0)
        {
            Console.WriteLine($"loading file {args[0]}");
            var source = File.ReadAllText(args[0]);
            var ast = parser.Parse(source);
            if (ast.IsOk)
            {
                program = ast.Result as LogSharpProgram;
            }
            else
            {
                Console.WriteLine($"source file {args[0]} is invalid.");
                foreach (var error in ast.Errors)
                {
                    Console.Error.WriteLine(error.ErrorMessage);
                }
                Environment.Exit(1);
            }
        }
        
        // The first argument is the main program (already parsed above); it is
        // handed to the interpreter so it is actually loaded. Any additional
        // arguments are extra program files loaded on top of it.
        Interpreter interpreter = new Interpreter(false, program, parser, new NestedTermUnifier());


        if (!string.IsNullOrEmpty(LogFile))
        {
            interpreter.DebugOutput = LogFile;
            interpreter.Verbose = true;
        }

        for (int i = 1; i < args.Length; i++)
        {
            if (!args[i].EndsWith(".dll"))
            {
                interpreter.LoadProgramFile(args[i]);
            }
        }

        // When stdin is a pipe or a file redirection, the REPL runs in
        // non-interactive mode: every solution is printed without asking the user
        // for confirmation. This allows piping a question into the CLI, e.g.
        //   echo "t(X)?" | Toplevel program.pl
        bool interactive = !Console.IsInputRedirected;

        while (true)
        {
            string input;
            if (interactive)
            {
                ReadLine.HistoryEnabled = true;
                input = ReadLine.Read("?. ");
            }
            else
            {
                // Console.ReadLine (and not ReadLine.Read, which requires a real
                // console) so piped input can be consumed.
                input = Console.In.ReadLine();
            }

            // End of stream (EOF) or explicit quit.
            if (input == null || input == "quit" || input == "exit")
            {
                return;
            }

            if (!string.IsNullOrEmpty(input))
            {
                ProcessInput(interpreter, parser, input, interactive);
            }
        }
    }


    private static void ProcessInput(Interpreter interpreter, Parser<LogSharpToken, LogSharpAST> parser,
        string input, bool interactive)
    {
        var query = parser.Parse(input, "rule");
        if (!query.IsOk)
        {
            Console.Error.WriteLine("bad query.");
            foreach (var error in query.Errors)
            {
                Console.Error.WriteLine(error.ErrorMessage);
            }
            return;
        }

        if (query.Result is Question ask)
        {
            if (ReplActions.ContainsKey(ask.Name))
            {
                ReplActions[ask.Name](interpreter, ask);
                return;
            }

            if (interactive)
            {
                // The solutions are enumerated lazily: the engine is suspended
                // between two solutions and only computes the next one when the user
                // explicitly asks for it. A query with many (or infinite) solutions
                // is therefore not fully evaluated up front.
                using (var answers = interpreter.SearchLazy(ask).GetEnumerator())
                {
                    while (answers.MoveNext())
                    {
                        var answer = answers.Current;
                        Console.WriteLine(answer);

                        // Ask whether to look for another solution after each one.
                        // Failed results ("don't know.") are not followed by a
                        // question since there is nothing more.
                        if (answer != null && answer.IsOk)
                        {
                            string more = ReadLine.Read("encore / stop ? ");
                            if (more == null ||
                                more.Trim().ToLowerInvariant() == "stop")
                            {
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                // Non-interactive (piped) mode: display every solution without
                // waiting for any validation.
                foreach (var answer in interpreter.SearchLazy(ask))
                {
                    Console.WriteLine(answer);
                }
            }
        }
        else if (query.Result is Rule rule)
        {
            interpreter.Append(rule);
        }
    }


    private static void UnifyTest(string source, string dest, string parserRule)
    {
        var parser = BuildParser();
        IUnifier unifier = new NestedTermUnifier();
        var r1 = parser.Parse(source, parserRule);
        var r2 = parser.Parse(dest, parserRule);
        if (r1.IsOk && r2.IsOk)
        {
            var t1 = r1.Result as Term;
            var t2 = r2.Result as Term;
            var u = unifier.UnifyTerms(t1, new Unification(), t2, new Unification());
            Console.WriteLine(source);
            Console.WriteLine(dest);
            Console.WriteLine(u);
        }
    }

    public static void Test()
    {
        UnifyTest("list(a,T)", "list(H,TAIL)", "term");
        UnifyTest("list(a,list(b,c))", "list(H,list(HEAD,TAIL))", "term"); // expect H=a HEAD=b tail = [c] | c
        UnifyTest("list(a,list(b,c))", "list(H,list(HEAD,TAIL))", "term");
//            "empty([]).",
////                    "const([a|Tail]).",
////                    "literal([a,b,c])."
        UnifyTest("[]", "[H|T]", "list");
        UnifyTest("[a,b]", "[H|T]", "list");
        UnifyTest("[a,b,c]", "[H|T]", "list");
        ;
    }


    public static void Run(string[] args)
    {
//            Test();
        Loop(args);
    }
}
