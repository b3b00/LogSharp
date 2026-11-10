using System;
using System.Linq;
using logSharp.interpretation;
using logSharp.interpretation.unification;
using logSharp.model;
using logSharp.parser;
using NFluent;
using sly.lexer;
using sly.parser.generator;
using Xunit;

namespace ogsharpTests
{
    public class ParserTest 
    {
        [Fact]
        public void TestDcgParse() 
        {
            var logParser = new LogSharpParser();
            var builder = new ParserBuilder<LogSharpToken, LogSharpAST>();
            var parser = builder.BuildParser(logParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "program");
            Check.That(parser.IsOk).IsTrue();
            var lexer = parser.Result.Lexer as GenericLexer<LogSharpToken>;
            CallBacksBuilder.BuildCallbacks<LogSharpToken>(lexer);
           
            var parsed = parser.Result.Parse(@"
statement(ast_assign(Var, ExprAST)) --> 
    [Var], [Eq], expr(ExprAST).

% Expressions (handling addition for simplicity)
expr(ast_add(Left, Right)) --> 
    term(Left), [+], expr(Right).

expr(AST) --> 
    term(AST).

% Terms: numbers or variables
term(ast_num(N)) --> 
    [N], { number(N) }.

term(ast_var(V)) --> 
    [V], { atom(V), V \= (=), V \= (+) }.
");
            Check.That(parsed.IsOk).IsTrue();
            Check.That(parsed.Result).IsNotNull();
            Check.That(parsed.Result).IsInstanceOf<LogSharpProgram>();
            var program = parsed.Result as LogSharpProgram;
            Check.That(program.Rules).CountIs(5);

            // Rule 0: statement
            var statementRule = program.Rules[0];
            Check.That(statementRule.IsPredicate).IsTrue();
            Check.That(statementRule.Name).IsEqualTo("statement");
            Check.That(statementRule.Arity).IsEqualTo(3);
            Check.That(statementRule.Clauses).CountIs(1);
            Check.That(statementRule.Clauses[0].Name).IsEqualTo("expr");
            Check.That(statementRule.Clauses[0].Arity).IsEqualTo(3);

            // Rule 1: expr(ast_add)
            var exprAddRule = program.Rules[1];
            Check.That(exprAddRule.IsPredicate).IsTrue();
            Check.That(exprAddRule.Name).IsEqualTo("expr");
            Check.That(exprAddRule.Arity).IsEqualTo(3);
            Check.That(exprAddRule.Clauses).CountIs(2);
            Check.That(exprAddRule.Clauses[0].Name).IsEqualTo("term");
            Check.That(exprAddRule.Clauses[1].Name).IsEqualTo("expr");

            // Rule 2: expr(AST)
            var exprRule = program.Rules[2];
            Check.That(exprRule.IsPredicate).IsTrue();
            Check.That(exprRule.Name).IsEqualTo("expr");
            Check.That(exprRule.Arity).IsEqualTo(3);
            Check.That(exprRule.Clauses).CountIs(1);
            Check.That(exprRule.Clauses[0].Name).IsEqualTo("term");

            // Rule 3: term(ast_num)
            var termNumRule = program.Rules[3];
            Check.That(termNumRule.IsPredicate).IsTrue();
            Check.That(termNumRule.Name).IsEqualTo("term");
            Check.That(termNumRule.Arity).IsEqualTo(3);
            Check.That(termNumRule.Clauses).CountIs(1);
            Check.That(termNumRule.Clauses[0].Name).IsEqualTo("number");

            // Rule 4: term(ast_var)
            var termVarRule = program.Rules[4];
            Check.That(termVarRule.IsPredicate).IsTrue();
            Check.That(termVarRule.Name).IsEqualTo("term");
            Check.That(termVarRule.Arity).IsEqualTo(3);
            Check.That(termVarRule.Clauses).CountIs(3);
            Check.That(termVarRule.Clauses[0].Name).IsEqualTo("atom");
            Check.That(termVarRule.Clauses[1].Name).IsEqualTo("assert");
            Check.That(termVarRule.Clauses[2].Name).IsEqualTo("assert");
        }

        [Fact]
        public void TestDcgFacts()
        {
            var logParser = new LogSharpParser();
            var builder = new ParserBuilder<LogSharpToken, LogSharpAST>();
            var parser = builder.BuildParser(logParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "program");
            Check.That(parser.IsOk).IsTrue();
            var lexer = parser.Result.Lexer as GenericLexer<LogSharpToken>;
            CallBacksBuilder.BuildCallbacks<LogSharpToken>(lexer);

            var parsed = parser.Result.Parse(@"
digit(zero) --> [0].
greeting() --> [hello, world].
empty() --> [].
empty2() --> .
");
            Check.That(parsed.IsOk).IsTrue();
            var program = parsed.Result as LogSharpProgram;
            Check.That(program.Rules).CountIs(4);

            // digit(zero) --> [0]. => Fact digit(zero, [0|S], S).
            var digitRule = program.Rules[0];
            Check.That(digitRule.IsFact).IsTrue();
            Check.That(digitRule.Name).IsEqualTo("digit");
            Check.That(digitRule.Arity).IsEqualTo(3);

            // greeting --> [hello, world]. => Fact greeting([hello, world|S], S).
            var greetingRule = program.Rules[1];
            Check.That(greetingRule.IsFact).IsTrue();
            Check.That(greetingRule.Name).IsEqualTo("greeting");
            Check.That(greetingRule.Arity).IsEqualTo(2);

            // empty --> []. => Fact empty(S, S).
            var emptyRule = program.Rules[2];
            Check.That(emptyRule.IsFact).IsTrue();
            Check.That(emptyRule.Name).IsEqualTo("empty");
            Check.That(emptyRule.Arity).IsEqualTo(2);

            // empty2 --> . => Fact empty2(S, S).
            var empty2Rule = program.Rules[3];
            Check.That(empty2Rule.IsFact).IsTrue();
            Check.That(empty2Rule.Name).IsEqualTo("empty2");
            Check.That(empty2Rule.Arity).IsEqualTo(2);
            
        }

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

            var parsed = parser.Result.Parse(@"phrase(statement(AST), [x, '=', 10, '+', 20])?","rule");
            Check.That(parsed.IsOk).IsTrue();
        }
    }
    
    
}
