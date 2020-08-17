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
        public ILogSharpAst Program(List<ILogSharpAst> rules)
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
        public ILogSharpAst FactRule(ILogSharpAst question)
        {
            return question;
        }

        [Production("rule : fact")]
        public ILogSharpAst QuestionRule(ILogSharpAst fact)
        {
            return fact;
        }

        [Production("rule : predicate")]
        public ILogSharpAst PredicateRule(Rule predicate)
        {
            return predicate;
        }
        
        #endregion
        
        #region predicates

        [Production("predicate : term Imply[d] term* Dot[d]")]
        public ILogSharpAst Predicate(ILogSharpAst head, List<ILogSharpAst> clauses)
        {
            return new Predicate(head as Term, clauses.Cast<Term>().ToList());
        }
        
        #endregion

        #region facts

        [Production("fact : term Dot[d]")]
        public ILogSharpAst TermFact(ILogSharpAst clause)
        {
            return new Fact(clause as Term);
        }


        [Production("fact : Atom Dot[d]")]
        public ILogSharpAst AtomFact(Token<LogSharpToken> atom)
        {
            return new Fact(atom.Value);
        }

        #endregion

        #region question

        [Production("question : term QuestionMark[d]")]
        public ILogSharpAst Question(ILogSharpAst clause)
        {
            return new Question(clause as Term);
        }

        #endregion

        #region lists

        [Production("list :  LeftBrack[d]   RightBrack[d]")]
        public ILogSharpAst EmptyList()
        {
            return model.Term.EmptyListTerm();
        }

        [Production("list :  LeftBrack[d] scalarorTerm Pipe[d] scalarorTerm  RightBrack[d]")]
        public ILogSharpAst ListTermConstructor(ILogSharpAst head, ILogSharpAst tail)
        {
            return model.Term.ListTerm(head as IValuable, tail as IValuable);
        }

        [Production("list :  LeftBrack[d] scalarorTerm (Comma[d] scalarorTerm)*  RightBrack[d]")]
        public ILogSharpAst ListTermLiteral(ILogSharpAst headParam, List<Group<LogSharpToken, ILogSharpAst>> tailParams)
        {
            List<IValuable> parameters = new List<IValuable>();
            parameters.Add(headParam as IValuable);
            foreach (var paramGroup in tailParams)
            {
                var param = paramGroup.Value(0) as IValuable;
                parameters.Add(param);
            }


            return model.Term.LiteralListTerm(parameters);
        }

        #endregion


        #region terms

        
        [Production("term : Variable Is[d]  LogSharpParser_expressions")]
        public ILogSharpAst Is(Token<LogSharpToken> variableToken,/*Token<LogSharpToken> isToken,*/ ILogSharpAst expression)
        {
            if (expression is IExpression expr)
            {
                var variable = new Variable(variableToken.Value); 
                return new Term(IsBuiltin.IsBuiltinName, variable, expr);
            }

            return null;
        }
        
        [Production("term : LogSharpParser_expressions")]
        public ILogSharpAst Assert(ILogSharpAst expr)
        {
            if (expr is IExpression expression)
            {
                return new Term(AssertBuiltin.AssertBuiltinName,expression);
            }

            return null;
        }
        
        [Production("term : Atom LeftPar[d]  RightPar[d]")]
        public ILogSharpAst TermNoArg(Token<LogSharpToken> clauseName)
        {
            List<IValuable> parameters = new List<IValuable>();

            return new Term(
                clauseName.StringWithoutQuotes
            );
        }
        
        [Production("term :  LeftPar[d]  RightPar[d]")]
        public ILogSharpAst NilTerm()
        {
            return Nil.Instance;
        }

        [Production("term: Nil[d]")]
        public ILogSharpAst NilKeywordTerm()
        {
            return Nil.Instance;
        }

        [Production("term : Atom LeftPar[d] scalarorTerm (Comma[d] scalarorTerm)*  RightPar[d]")]
        public ILogSharpAst Term(Token<LogSharpToken> clauseName, ILogSharpAst headParam,
            IEnumerable<Group<LogSharpToken, ILogSharpAst>> tailParams)
        {
                var parameters = new List<IValuable>();
                parameters.Add(headParam as IValuable);
                foreach (var paramGroup in tailParams)
                {
                    var param = paramGroup.Value(0) as IValuable;
                    parameters.Add(param);
                }

                return new Term(
                    clauseName.StringWithoutQuotes,
                    parameters.ToArray()
                );
        }
        
        #endregion

        #region scalars terms and expressions
        
        [Production("scalarorTerm : LogSharpParser_expressions")]
        public ILogSharpAst ScalarOrTermExpression(ILogSharpAst scal)
        {
            return scal;
        }

        [Production("scalarorTerm : term")]
        public ILogSharpAst ScalarOrTermTerm(ILogSharpAst term)
        {
            return term;
        }

        [Production("scalarorTerm : list")]
        public ILogSharpAst ScalarOrTermList(ILogSharpAst list)
        {
            return list;
        }
        
        #endregion

    #region scalars
        
        
        [Production("scalar : Variable")]
        public ILogSharpAst AtomVariable(Token<LogSharpToken> variable)
        {
            return new Variable(variable.Value);
        }

        [Production("scalar : Atom")]
        public ILogSharpAst AtomScalar(Token<LogSharpToken> atom)
        {
            return new Atom(atom.Value);
        }

        [Production("scalar : String")]
        public ILogSharpAst StringScalar(Token<LogSharpToken> stringToken)
        {
            return new LogString(stringToken.StringWithoutQuotes);
        }

        [Production("scalar : Integer")]
        public ILogSharpAst IntScalar(Token<LogSharpToken> intToken)
        {
            return new LogInt(intToken.IntValue);
        }

        [Production("scalar : Double")]
        public ILogSharpAst DoubleScalar(Token<LogSharpToken> doubleToken)
        {
            return new LogDouble(doubleToken.DoubleValue);
        }

        #endregion

        #region expressions

        [Operation((int) LogSharpToken.Plus, Affix.InFix, Associativity.Right, 10)]
        [Operation((int) LogSharpToken.Minus, Affix.InFix, Associativity.Left, 10)]
        public ILogSharpAst BinaryTermExpression(ILogSharpAst left, Token<LogSharpToken> operation, ILogSharpAst right)
        {
            return new BinaryOperation(left, operation.TokenID, right);
        }


        [Operation((int) LogSharpToken.Times, Affix.InFix, Associativity.Right, 50)]
        [Operation((int) LogSharpToken.Divide, Affix.InFix, Associativity.Left, 50)]
        public ILogSharpAst BinaryFactorExpression(ILogSharpAst left, Token<LogSharpToken> operation, ILogSharpAst right)
        {
            return new BinaryOperation(left, operation.TokenID, right);
        }


        [Operation((int) LogSharpToken.Minus, Affix.PreFix, Associativity.Right, 100)]
        public ILogSharpAst PreFixExpression(Token<LogSharpToken> operation, ILogSharpAst value)
        {
            return new UnaryOperation(operation.TokenID, value);
        }


        [Operation((int) LogSharpToken.EqualsTo, Affix.InFix, Associativity.Right, 5)]
        [Operation((int) LogSharpToken.Different, Affix.InFix, Associativity.Left, 5)]
        [Operation((int) LogSharpToken.Gt, Affix.InFix, Associativity.Left, 5)]
        [Operation((int) LogSharpToken.Lt, Affix.InFix, Associativity.Left, 5)]
        [Operation((int) LogSharpToken.Gte, Affix.InFix, Associativity.Left, 5)]
        [Operation((int) LogSharpToken.Lte, Affix.InFix, Associativity.Left, 5)]
        public ILogSharpAst BinaryComparisonExpression(ILogSharpAst left, Token<LogSharpToken> operation,
            ILogSharpAst right)
        {
            return new BinaryOperation(left, operation.TokenID, right);
        }


        [Operand]
        [Production("operand : primary_value")]
        public ILogSharpAst OperandValue(ILogSharpAst value)
        {
            return value;
        }


        [Production("primary_value : Variable")]
        public ILogSharpAst AtomExpressionVariable(Token<LogSharpToken> variable)
        {
            return new Variable(variable.Value);
        }

        [Production("primary_value : Atom")]
        public ILogSharpAst AtomExpressionScalar(Token<LogSharpToken> atom)
        {
            return new Atom(atom.Value);
        }

        [Production("primary_value : String")]
        public ILogSharpAst StringExpressionScalar(Token<LogSharpToken> stringToken)
        {
            return new LogString(stringToken.StringWithoutQuotes);
        }

        [Production("primary_value : Integer")]
        public ILogSharpAst IntExpressionScalar(Token<LogSharpToken> intToken)
        {
            return new LogInt(intToken.IntValue);
        }

        [Production("primary_value : Double")]
        public ILogSharpAst DoubleExpressionScalar(Token<LogSharpToken> doubleToken)
        {
            return new LogDouble(doubleToken.DoubleValue);
        }

        [Production("primary_value : LPAREN SimpleExpressionParserWithContext_expressions RPAREN")]
        public ILogSharpAst OperandParens(Token<LogSharpToken> lparen, ILogSharpAst value, Token<LogSharpToken> rparen)
        {
            return value;
        }

        #endregion
    }
}