using sly.parser.generator;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using logSharp.model;
using sly.lexer;
using sly.parser.parser;

namespace logSharp.parser
{
    public class LogSharpParser
    {

        [Production("program : rule *")]
        public ILogSharpAst program(List<ILogSharpAst> rules)
        {
            return new LogSharpProgram(rules.Cast<Rule>().ToList());
        }

        [Production("rule : question")]
        public ILogSharpAst factRule(ILogSharpAst question)
        {
            return question;
        }
        
        [Production("rule : fact")]
        public ILogSharpAst questionRule(ILogSharpAst fact)
        {
            return fact;
        }
        
        [Production("rule : predicate")]
        public ILogSharpAst predicateRule(Rule predicate)
        {
            return predicate;
        }

        [Production("predicate : term Imply[d] term* Dot[d]")]
        public ILogSharpAst Predicate(ILogSharpAst head, List<ILogSharpAst> clauses)
        {
            return new Predicate(head as Term, clauses.Cast<Term>().ToList());
        }


        [Production("fact : term Dot[d]")]
        public ILogSharpAst Fact(ILogSharpAst clause)
        {
            return new Fact(clause as Term);
        }
        
        [Production("question : term QuestionMark[d]")]
        public ILogSharpAst Question(ILogSharpAst clause)
        {
            return new Question(clause as Term);
        }
        
        [Production("fact : Atom Dot[d]")]
        public ILogSharpAst fact(Token<LogSharpToken> atom)
        {
            return new Fact(atom.Value);
        }

        
        [Production("list :  LeftBrack[d]   RightBrack[d]")]
        public ILogSharpAst EmptyList()
        {
            return Term.EmptyListTerm();
        }

        [Production("list :  LeftBrack[d] scalarorTerm Pipe[d] scalarorTerm  RightBrack[d]")]
        public ILogSharpAst ListTermConstructor(ILogSharpAst head, ILogSharpAst tail)
        {
            return Term.ListTerm( new List<IValuable>() {head as IValuable, tail as IValuable});
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

            
            
            return Term.ListTerm(
                parameters
            );
        }

        
        [Production("term : Atom LeftPar[d]  RightPar[d]")]
        public ILogSharpAst termNoArg(Token<LogSharpToken> clauseName )
        {
            List<IValuable> parameters = new List<IValuable>();
            
            return new Term(
                clauseName.StringWithoutQuotes,
                parameters
            );
        }

        [Production("term : Atom LeftPar[d] scalarorTerm (Comma[d] scalarorTerm)*  RightPar[d]")]
        public ILogSharpAst term(Token<LogSharpToken> clauseName, ILogSharpAst headParam, List<Group<LogSharpToken, ILogSharpAst>> tailParams)
        {
            List<IValuable> parameters = new List<IValuable>();
            parameters.Add(headParam as IValuable);
            foreach (var paramGroup in tailParams)
            {
                var param = paramGroup.Value(0) as IValuable;
                parameters.Add(param);
            }

            return new Term(
                clauseName.StringWithoutQuotes,
                parameters
                );
        }

        [Production("scalarorTerm : scalar")]
        public ILogSharpAst scalarOrTermScalar(ILogSharpAst scal)
        {
            return scal;
        }
        
        [Production("scalarorTerm : term")]
        public ILogSharpAst scalarOrTermTerm(ILogSharpAst term)
        {
            return term;
        }
        
        [Production("scalarorTerm : list")]
        public ILogSharpAst scalarOrTermList(ILogSharpAst list)
        {
            return list;
        }
        
        
        [Production("scalar : Variable")]
        public ILogSharpAst AtomVariable(Token<LogSharpToken> atom)
        {
            return new Variable(atom.Value);
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
        
        
        
    }
}