using System.Linq;
using logSharp.interpretation;
using logSharp.interpretation.unification;
using logSharp.model;
using logSharp.parser;
using NFluent;
using sly.lexer;
using sly.parser.generator;
using Xunit;

namespace ogsharpTests;

public class DcgTests
{
    [Fact]
    public void TestDcgExecution()
    {
        var logParser = new LogSharpParser();
        var builder = new ParserBuilder<LogSharpToken, LogSharpAST>();
        var parser = builder.BuildParser(logParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "program");
        Check.That(parser.IsOk).IsTrue();
        var lexer = parser.Result.Lexer as GenericLexer<LogSharpToken>;
        CallBacksBuilder.BuildCallbacks<LogSharpToken>(lexer);

        var parsed = parser.Result.Parse(@"
digit(zero) --> [0].
digit(one) --> [1].
number_pair(X, Y) --> digit(X), digit(Y).
");
        Check.That(parsed.IsOk).IsTrue();
        var program = parsed.Result as LogSharpProgram;

        var interpreter = new Interpreter(false, program, parser.Result, new NestedTermUnifier());
        var questionParse = parser.Result.Parse("number_pair(A, B, [0, 1], [])?", "question");
        Check.That(questionParse.IsOk).IsTrue();
        var answers = interpreter.Search(questionParse.Result as Question).ToList();
        Check.That(answers).CountIs(1);
        Check.That(answers[0].IsOk).IsTrue();
        Check.That(answers[0].GetBinding("A").ToString()).IsEqualTo("zero");
        Check.That(answers[0].GetBinding("B").ToString()).IsEqualTo("one");
    }
        
        
    [Fact]
    public void TestQueryWithAtomicStrings()
    {
        var logParser = new LogSharpParser();
        var builder = new ParserBuilder<LogSharpToken, LogSharpAST>();
        var parser = builder.BuildParser(logParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "program");
        Check.That(parser.IsOk).IsTrue();
        var lexer = parser.Result.Lexer as GenericLexer<LogSharpToken>;
        CallBacksBuilder.BuildCallbacks<LogSharpToken>(lexer);

        var parsedProgram = parser.Result.Parse(@"
% A statement: variable = expression
statement(ast_assign(Var, ExprAST)) --> 
    [Var], ['='], expr(ExprAST).

% Expressions (handling addition for simplicity)
expr(ast_add(Left, Right)) --> 
    term(Left), ['+'], expr(Right).
expr(AST) --> 
    term(AST).

% Terms: numbers or variables
term(ast_num(N)) --> 
    [N], { number(N) }.
term(ast_var(V)) --> 
    [V], { atom(V), V \= (=), V \= (+) }.
");
        Check.That(parsedProgram.IsOk).IsTrue();
        var program = parsedProgram.Result as LogSharpProgram;

        Interpreter interpreter = new Interpreter(false, program, parser.Result, new NestedTermUnifier());
            
        var parsed = parser.Result.Parse(@"phrase(statement(AST), [x, '=', 10, '+', 20])?","rule");
        Check.That(parsed.IsOk).IsTrue();

        var searchResult = interpreter.Search(parsed.Result as Question);
        Check.That(searchResult).IsNotNull();
        Check.That(searchResult).CountIs(1);
        Check.That(searchResult[0].IsOk).IsTrue();

        // AST = ast_assign(x, ast_add(ast_num(10), ast_num(20)))
        var ast = searchResult[0].GetBinding("AST") as Term;
        Check.That(ast).IsNotNull();
        Check.That(ast.Name).IsEqualTo("ast_assign");
        Check.That(ast.Parameters).CountIs(2);

        // ast_assign(x, ...) : assigned variable is the atom x
        Check.That(ast.Parameters[0]).IsInstanceOf<Atom>();
        Check.That(((Atom) ast.Parameters[0]).Name).IsEqualTo("x");

        // ast_assign(..., ast_add(...)) : expression is a binary addition
        var add = ast.Parameters[1] as Term;
        Check.That(add).IsNotNull();
        Check.That(add.Name).IsEqualTo("ast_add");
        Check.That(add.Parameters).CountIs(2);

        // ast_add(ast_num(10), ...)
        var left = add.Parameters[0] as Term;
        Check.That(left).IsNotNull();
        Check.That(left.Name).IsEqualTo("ast_num");
        Check.That(left.Parameters).CountIs(1);
        Check.That(left.Parameters[0]).IsInstanceOf<Int>();
        Check.That(((Int) left.Parameters[0]).Value).IsEqualTo(10);

        // ast_add(..., ast_num(20))
        var right = add.Parameters[1] as Term;
        Check.That(right).IsNotNull();
        Check.That(right.Name).IsEqualTo("ast_num");
        Check.That(right.Parameters).CountIs(1);
        Check.That(right.Parameters[0]).IsInstanceOf<Int>();
        Check.That(((Int) right.Parameters[0]).Value).IsEqualTo(20);


    }        
}