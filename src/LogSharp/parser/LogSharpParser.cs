using System;
using sly.parser.generator;
using System.Collections.Generic;
using System.Linq;
using logSharp.builtins.expressions;
using logSharp.model;
using sly.lexer;
using sly.parser.parser;

namespace logSharp.parser
{
    public class LogSharpParser
    {
        [Production("program : rule *")]
        public LogSharpAST Program(List<LogSharpAST> rules)
        {
            try
            {
                return new LogSharpProgram(rules.Cast<Rule>().ToList());
            }
            catch (Exception e)
            {
                return null;
            }
        }

        #region rules
        
        [Production("rule : question")]
        public LogSharpAST FactRule(LogSharpAST question)
        {
            return question;
        }

        [Production("rule : fact")]
        public LogSharpAST QuestionRule(LogSharpAST fact)
        {
            return fact;
        }

        [Production("rule : predicate")]
        public LogSharpAST PredicateRule(Rule predicate)
        {
            return predicate;
        }
        
        #endregion
        
        #region predicates

        [Production("predicate : term Imply[d] term* Dot[d]")]
        public LogSharpAST Predicate(LogSharpAST head, List<LogSharpAST> clauses)
        {
            return new Predicate(head as Term, clauses.Cast<Term>().ToList());
        }
        
        #endregion

        #region facts

        [Production("fact : term Dot[d]")]
        public LogSharpAST TermFact(LogSharpAST clause)
        {
            return new Fact(clause as Term);
        }


        [Production("fact : Atom Dot[d]")]
        public LogSharpAST AtomFact(Token<LogSharpToken> atom)
        {
            return new Fact(atom.Value);
        }

        #endregion

        #region question

        [Production("question : term QuestionMark[d]")]
        public LogSharpAST Question(LogSharpAST clause)
        {
            return new Question(clause as Term);
        }

        #endregion

        #region lists

        [Production("list :  LeftBrack[d]   RightBrack[d]")]
        public LogSharpAST EmptyList()
        {
            return model.Term.EmptyListTerm();
        }

        [Production("list :  LeftBrack[d] scalarorTerm Pipe[d] scalarorTerm  RightBrack[d]")]
        public LogSharpAST ListTermConstructor(LogSharpAST head, LogSharpAST tail)
        {
            return model.Term.ListTerm(head as Valuable, tail as Valuable);
        }

        [Production("list :  LeftBrack[d] scalarorTerm (Comma[d] scalarorTerm)*  RightBrack[d]")]
        public LogSharpAST ListTermLiteral(LogSharpAST headParam, List<Group<LogSharpToken, LogSharpAST>> tailParams)
        {
            List<Valuable> parameters = new List<Valuable>();
            parameters.Add(headParam as Valuable);
            foreach (var paramGroup in tailParams)
            {
                var param = paramGroup.Value(0) as Valuable;
                parameters.Add(param);
            }


            return model.Term.ListTerm(parameters);
        }

        #endregion


        #region terms

        
        [Production("term : Variable Is[d]  LogSharpParser_expressions")]
        public LogSharpAST Is(Token<LogSharpToken> variableToken,/*Token<LogSharpToken> isToken,*/ LogSharpAST expression)
        {
            if (expression is IExpression expr)
            {
                var variable = new Variable(variableToken.Value); 
                return new Term(IsBuiltin.IsBuiltinName, new List<Valuable>(){variable, expr});
            }

            return null;
        }
        
        [Production("term : LogSharpParser_expressions")]
        public LogSharpAST Assert(LogSharpAST expr)
        {
            if (expr is IExpression)
            {
                return new Term(AssertBuiltin.AssertBuiltinName,new List<Valuable>(){expr as Valuable});
            }

            return expr;
        }
        
        [Production("term : Atom LeftPar[d]  RightPar[d]")]
        public LogSharpAST TermNoArg(Token<LogSharpToken> clauseName)
        {
            List<Valuable> parameters = new List<Valuable>();

            return new Term(
                clauseName.StringWithoutQuotes
            );
        }
        
        [Production("term : Atom LeftPar[d] scalarorTerm (Comma[d] scalarorTerm)*  RightPar[d]")]
        public LogSharpAST Term(Token<LogSharpToken> clauseName, LogSharpAST headParam,
            List<Group<LogSharpToken, LogSharpAST>> tailParams)
        {
                var parameters = new List<Valuable>();
                parameters.Add(headParam as Valuable);
                foreach (var paramGroup in tailParams)
                {
                    var param = paramGroup.Value(0) as Valuable;
                    parameters.Add(param);
                }

                return new Term(
                    clauseName.StringWithoutQuotes,
                    parameters);
        }


        [Production("term: Cut[d]")]
        public LogSharpAST CutTerm()
        {
            return new Term("cut");
        }
        
        #endregion

        #region scalars terms and expressions
        
        [Production("scalarorTerm : LogSharpParser_expressions")]
        public LogSharpAST ScalarOrTermExpression(LogSharpAST scal)
        {
            return scal;
        }

        [Production("scalarorTerm : term")]
        public LogSharpAST ScalarOrTermTerm(LogSharpAST term)
        {
            return term;
        }

        [Production("scalarorTerm : list")]
        public LogSharpAST ScalarOrTermList(LogSharpAST list)
        {
            return list;
        }
        
        #endregion

    #region scalars
        
        
        [Production("scalar : Variable")]
        public LogSharpAST AtomVariable(Token<LogSharpToken> variable)
        {
            return new Variable(variable.Value);
        }

        [Production("scalar : Atom")]
        public LogSharpAST AtomScalar(Token<LogSharpToken> atom)
        {
            return new Atom(atom.Value);
        }

        [Production("scalar : String")]
        public LogSharpAST StringScalar(Token<LogSharpToken> stringToken)
        {
            return new logSharp.model.String(stringToken.StringWithoutQuotes);
        }

        [Production("scalar : Integer")]
        public LogSharpAST IntScalar(Token<LogSharpToken> intToken)
        {
            return new Int(intToken.IntValue);
        }

        [Production("scalar : Double")]
        public LogSharpAST DoubleScalar(Token<LogSharpToken> doubleToken)
        {
            return new logSharp.model.Double(doubleToken.DoubleValue);
        }

        #endregion

        #region expressions

        [Operation((int) LogSharpToken.Plus, Affix.InFix, Associativity.Right, 10)]
        [Operation((int) LogSharpToken.Minus, Affix.InFix, Associativity.Left, 10)]
        public LogSharpAST BinaryTermExpression(LogSharpAST left, Token<LogSharpToken> operation, LogSharpAST right)
        {
            return new BinaryOperation(left, operation.TokenID, right);
        }


        [Operation((int) LogSharpToken.Times, Affix.InFix, Associativity.Right, 50)]
        [Operation((int) LogSharpToken.Divide, Affix.InFix, Associativity.Left, 50)]
        public LogSharpAST BinaryFactorExpression(LogSharpAST left, Token<LogSharpToken> operation, LogSharpAST right)
        {
            return new BinaryOperation(left, operation.TokenID, right);
        }


        [Operation((int) LogSharpToken.Minus, Affix.PreFix, Associativity.Right, 100)]
        public LogSharpAST PreFixExpression(Token<LogSharpToken> operation, LogSharpAST value)
        {
            return new UnaryOperation(operation.TokenID, value);
        }


        [Operation((int) LogSharpToken.EqualsTo, Affix.InFix, Associativity.Right, 5)]
        [Operation((int) LogSharpToken.Different, Affix.InFix, Associativity.Left, 5)]
        [Operation((int) LogSharpToken.Gt, Affix.InFix, Associativity.Left, 5)]
        [Operation((int) LogSharpToken.Lt, Affix.InFix, Associativity.Left, 5)]
        [Operation((int) LogSharpToken.Gte, Affix.InFix, Associativity.Left, 5)]
        [Operation((int) LogSharpToken.Lte, Affix.InFix, Associativity.Left, 5)]
        public LogSharpAST BinaryComparisonExpression(LogSharpAST left, Token<LogSharpToken> operation,
            LogSharpAST right)
        {
            return new BinaryOperation(left, operation.TokenID, right);
        }


        [Operand]
        [Production("operand : primary_value")]
        public LogSharpAST OperandValue(LogSharpAST value)
        {
            return value;
        }


        [Production("primary_value : Variable")]
        public LogSharpAST AtomExpressionVariable(Token<LogSharpToken> variable)
        {
            return new Variable(variable.Value);
        }

        [Production("primary_value : Atom")]
        public LogSharpAST AtomExpressionScalar(Token<LogSharpToken> atom)
        {
            return new Atom(atom.Value);
        }

        [Production("primary_value : String")]
        public LogSharpAST StringExpressionScalar(Token<LogSharpToken> stringToken)
        {
            return new logSharp.model.String(stringToken.StringWithoutQuotes);
        }

        [Production("primary_value : Integer")]
        public LogSharpAST IntExpressionScalar(Token<LogSharpToken> intToken)
        {
            return new Int(intToken.IntValue);
        }

        [Production("primary_value : Double")]
        public LogSharpAST DoubleExpressionScalar(Token<LogSharpToken> doubleToken)
        {
            return new logSharp.model.Double(doubleToken.DoubleValue);
        }

        [Production("primary_value : LeftPar LogSharpParser_expressions RightPar")]
        public LogSharpAST OperandParens(Token<LogSharpToken> lparen, LogSharpAST value, Token<LogSharpToken> rparen)
        {
            return value;
        }

        #endregion
    }
}