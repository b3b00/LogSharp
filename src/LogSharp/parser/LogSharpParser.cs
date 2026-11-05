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
        public LogSharpAST program(List<LogSharpAST> rules)
        {
            return new LogSharpProgram(rules.Cast<Rule>().ToList());
        }

        [Production("rule : question")]
        public LogSharpAST factRule(LogSharpAST question)
        {
            return question;
        }
        
        [Production("rule : fact")]
        public LogSharpAST questionRule(LogSharpAST fact)
        {
            return fact;
        }
        
        [Production("rule : predicate")]
        public LogSharpAST predicateRule(Rule predicate)
        {
            return predicate;
        }

        [Production("predicate : term Imply[d] term* Dot[d]")]
        public LogSharpAST Predicate(LogSharpAST head, List<LogSharpAST> clauses)
        {
            return new Predicate(head as Term, clauses.Cast<Term>().ToList());
        }


        [Production("fact : term Dot[d]")]
        public LogSharpAST Fact(LogSharpAST clause)
        {
            return new Fact(clause as Term);
        }
        
        [Production("question : term QuestionMark[d]")]
        public LogSharpAST Question(LogSharpAST clause)
        {
            return new Question(clause as Term);
        }
        
        [Production("fact : [Atom|AtomicString] Dot[d]")]
        public LogSharpAST fact(Token<LogSharpToken> atom)
        {
            return new Fact(atom.StringWithoutQuotes);
        }
        
        

        
        [Production("list :  LeftBrack[d]   RightBrack[d]")]
        public LogSharpAST EmptyList()
        {
            return Term.EmptyListTerm();
        }

        [Production("list :  LeftBrack[d] scalarorTerm Pipe[d] scalarorTerm  RightBrack[d]")]
        public LogSharpAST ListTermConstructor(LogSharpAST head, LogSharpAST tail)
        {
            return Term.ListTerm( new List<Valuable>() {head as Valuable, tail as Valuable});
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

            
            
            return Term.ListTerm(
                parameters
            );
        }

        
        [Production("term : [Atom|AtomicString] LeftPar[d]  RightPar[d]")]
        public LogSharpAST termNoArg(Token<LogSharpToken> clauseName )
        {
            List<Valuable> parameters = new List<Valuable>();
            
            return new Term(
                clauseName.StringWithoutQuotes,
                parameters
            );
        }

        [Production("term : [Atom|AtomicString] LeftPar[d] scalarorTerm (Comma[d] scalarorTerm)*  RightPar[d]")]
        public LogSharpAST term(Token<LogSharpToken> clauseName, LogSharpAST headParam, List<Group<LogSharpToken, LogSharpAST>> tailParams)
        {
            List<Valuable> parameters = new List<Valuable>();
            parameters.Add(headParam as Valuable);
            foreach (var paramGroup in tailParams)
            {
                var param = paramGroup.Value(0) as Valuable;
                parameters.Add(param);
            }

            return new Term(
                clauseName.StringWithoutQuotes,
                parameters
                );
        }

        [Production("scalarorTerm : scalar")]
        public LogSharpAST scalarOrTermScalar(LogSharpAST scal)
        {
            return scal;
        }
        
        [Production("scalarorTerm : term")]
        public LogSharpAST scalarOrTermTerm(LogSharpAST term)
        {
            return term;
        }
        
        [Production("scalarorTerm : list")]
        public LogSharpAST scalarOrTermList(LogSharpAST list)
        {
            return list;
        }
        
        
        [Production("scalar : Variable")]
        public LogSharpAST AtomVariable(Token<LogSharpToken> variable)
        {
            return new Variable(variable.Value);
        }

        [Production("scalar : [Atom|AtomicString]")]
        public LogSharpAST AtomScalar(Token<LogSharpToken> atom)
        {
            return new Atom(atom.StringWithoutQuotes);
        }
        [Production("scalar : String")]
        public LogSharpAST StringScalar(Token<LogSharpToken> stringToken)
        {
            return new String(stringToken.StringWithoutQuotes);
        }
        
        [Production("scalar : Integer")]
        public LogSharpAST IntScalar(Token<LogSharpToken> intToken)
        {
            return new Int(intToken.IntValue);
        }
        /*
        [Production("scalar : Double")]
        public LogSharpAST DoubleScalar(Token<LogSharpToken> doubleToken)
        {
            return new LogDouble(doubleToken.DoubleValue);
        }*/
        
        
        
    }
}