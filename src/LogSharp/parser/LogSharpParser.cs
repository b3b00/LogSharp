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
        private readonly DcgTranslator _dcgTranslator = new DcgTranslator();

        [Production("program : [rule|dcg] *")]
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

        [Production("list :  LeftBrack[d] listElement (Comma[d] listElement)* (Pipe[d] scalarorTerm)?  RightBrack[d]")]
        public LogSharpAST ListTermLiteral(LogSharpAST headParam, List<Group<LogSharpToken, LogSharpAST>> tailParams,
            ValueOption<Group<LogSharpToken, LogSharpAST>> pipeTail)
        {
            List<Valuable> parameters = new List<Valuable>();
            parameters.Add(headParam as Valuable);
            foreach (var paramGroup in tailParams)
            {
                var param = paramGroup.Value(0) as Valuable;
                parameters.Add(param);
            }

            Valuable result;
            Valuable tailValue = pipeTail.Match(
                group => (Valuable)group.Value(0),
                () => null);
            if (tailValue != null)
            {
                // build [X1, ..., Xn | Tail] as X1 | [X2 | ... | [Xn | Tail]]
                result = tailValue;
                for (int i = parameters.Count - 1; i >= 0; i--)
                {
                    result = model.Term.ListTerm(parameters[i], result);
                }
            }            else
            {
                result = model.Term.ListTerm(parameters);
            }

            return result;
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

        [Production("scalar : [Atom | AtomicString]")]
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
        [Operation((int) LogSharpToken.NotUnifiable, Affix.InFix, Associativity.Left, 5)]
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

        [Production("primary_value : LeftPar[d] Equal[d] RightPar[d]")]
        public LogSharpAST EqualOperand()
        {
            return new Atom("=");
        }

        [Production("primary_value : LeftPar[d] Plus[d] RightPar[d]")]
        public LogSharpAST PlusOperand()
        {
            return new Atom("+");
        }

        #endregion
        
        #region DCG

        [Production("dcg : term DcgArrow[d] Dot[d]")]
        public LogSharpAST DcgEmpty(LogSharpAST head)
        {
            var prolog = _dcgTranslator.TranslateDcg(head, new List<IDcgClause>());
            return prolog;
        }

        [Production("dcg : term DcgArrow[d] dcgClause (Comma[d] dcgClause)* Dot[d]")]
        public LogSharpAST DcgRule(LogSharpAST head, LogSharpAST headClause, List<Group<LogSharpToken, LogSharpAST>> tailClauses)
        {
            var clauses = new List<IDcgClause>();
            if (headClause is IDcgClause c)
            {
                clauses.Add(c);
            }
            if (tailClauses != null)
            {
                foreach (var group in tailClauses)
                {
                    if (group.Value(0) is IDcgClause tc)
                    {
                        clauses.Add(tc);
                    }
                }
            }
            var prolog = _dcgTranslator.TranslateDcg(head, clauses);
            return prolog;
            ;
        }

        [Production("dcgClause : term")]
        public LogSharpAST DcgNonTerminalClause(LogSharpAST clause)
        {
            if (clause is Term t)
            {
                if (t.Name == "cut" && (t.Parameters == null || !t.Parameters.Any()))
                {
                    return new DcgPrologGoals(new List<Term> { t });
                }
                return new DcgNonTerminal(t);
            }
            if (clause is Atom a)
            {
                return new DcgNonTerminal(new Term(a.Name));
            }
            return null;
        }

        [Production("dcgClause : LeftBrack[d] RightBrack[d]")]
        public LogSharpAST DcgTerminalEmpty()
        {
            return new DcgTerminal(new List<Valuable>());
        }

        [Production("dcgClause : LeftBrack[d] listElement (Comma[d] listElement)* RightBrack[d]")]
        public LogSharpAST DcgTerminalList(LogSharpAST head, List<Group<LogSharpToken, LogSharpAST>> tail)
        {
            var list = new List<Valuable>();
            if (head is Valuable v)
            {
                list.Add(v);
            }
            if (tail != null)
            {
                foreach (var group in tail)
                {
                    if (group.Value(0) is Valuable tv)
                    {
                        list.Add(tv);
                    }
                }
            }
            return new DcgTerminal(list);
        }

        [Production("listElement : scalarorTerm")]
        public LogSharpAST ListElement(LogSharpAST item) => item;

        [Production("listElement : Plus")]
        public LogSharpAST ListElementPlus(Token<LogSharpToken> tok) => new Atom("+");

        [Production("listElement : Minus")]
        public LogSharpAST ListElementMinus(Token<LogSharpToken> tok) => new Atom("-");

        [Production("listElement : Times")]
        public LogSharpAST ListElementTimes(Token<LogSharpToken> tok) => new Atom("*");

        [Production("listElement : Divide")]
        public LogSharpAST ListElementDivide(Token<LogSharpToken> tok) => new Atom("/");

        [Production("listElement : Equal")]
        public LogSharpAST ListElementEqual(Token<LogSharpToken> tok) => new Atom("=");

        [Production("listElement : AtomicString")]
        public LogSharpAST ListElementAtomicString(Token<LogSharpToken> tok) => new Atom(tok.StringWithoutQuotes);

        [Production("dcgClause : LeftCurly[d] RightCurly[d]")]
        public LogSharpAST DcgPrologEmpty()
        {
            return new DcgPrologGoals(new List<Term>());
        }

        [Production("dcgClause : LeftCurly[d] prologGoal (Comma[d] prologGoal)* RightCurly[d]")]
        public LogSharpAST DcgPrologList(LogSharpAST head, List<Group<LogSharpToken, LogSharpAST>> tail)
        {
            var list = new List<Term>();
            DcgTranslator.AddPrologGoal(list, head);
            if (tail != null)
            {
                foreach (var group in tail)
                {
                    DcgTranslator.AddPrologGoal(list, group.Value(0));
                }
            }
            return new DcgPrologGoals(list);
        }

        [Production("prologGoal : term")]
        public LogSharpAST PrologGoalTerm(LogSharpAST term) => term;

        #endregion
    }
}
