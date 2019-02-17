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
            return new LogProgram(rules.Cast<LogRule>().ToList());
        }

        [Production("rule : fact")]
        public LogSharpAST factRule(LogSharpAST fact)
        {
            return fact;
        }
        
        [Production("rule : predicate")]
        public LogSharpAST predicateRule(LogRule predicate)
        {
            return predicate;
        }

        [Production("predicate : term Imply[d] term* Dot[d]")]
        public LogSharpAST Predicate(LogSharpAST head, List<LogSharpAST> clauses)
        {
            return new LogPredicate(head as LogTerm, clauses.Cast<LogTerm>().ToList());
        }


        [Production("fact : term Dot[d]")]
        public LogSharpAST fact(LogSharpAST clause)
        {
            return new LogFact(clause as LogTerm);
        }
        
        [Production("fact : Atom Dot[d]")]
        public LogSharpAST fact(Token<LogSharpToken> atom)
        {
            return new LogFact(atom.Value);
        }

        
        [Production("list :  LeftBrack[d]   RightBrack[d]")]
        public LogSharpAST EmptyList()
        {
            return new LogList();
        }

        [Production("list :  LeftBrack[d] scalarorTerm Pipe[d] scalarorTerm  RightBrack[d]")]
        public LogSharpAST ListTermConst(LogSharpAST head, LogSharpAST tail)
        {
            return new LogTerm("list", new List<LogValuable>() {head as LogValuable, tail as LogValuable});
        }
        
        [Production("list :  LeftBrack[d] scalarorTerm (Comma[d] scalarorTerm)*  RightBrack[d]")]
        public LogSharpAST ListTermLiteral(LogSharpAST headParam, List<Group<LogSharpToken, LogSharpAST>> tailParams)
        {
            List<LogValuable> parameters = new List<LogValuable>();
            parameters.Add(headParam as LogValuable);
            foreach (var paramGroup in tailParams)
            {
                var param = paramGroup.Value(0) as LogValuable;
                parameters.Add(param);
            }

            
            
            return new LogList(
                parameters
            );
        }


        [Production("term : Atom LeftPar[d] scalarorTerm (Comma[d] scalarorTerm)*  RightPar[d]")]
        public LogSharpAST term(Token<LogSharpToken> clauseName, LogSharpAST headParam, List<Group<LogSharpToken, LogSharpAST>> tailParams)
        {
            List<LogValuable> parameters = new List<LogValuable>();
            parameters.Add(headParam as LogValuable);
            foreach (var paramGroup in tailParams)
            {
                var param = paramGroup.Value(0) as LogValuable;
                parameters.Add(param);
            }

            return new LogTerm(
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
        public LogSharpAST AtomVariable(Token<LogSharpToken> atom)
        {
            return new LogVariable(atom.Value);
        }

        [Production("scalar : Atom")]
        public LogSharpAST AtomScalar(Token<LogSharpToken> atom)
        {
            return new LogAtom(atom.Value);
        }
        /*[Production("scalar : String")]
        public LogSharpAST StringScalar(Token<LogSharpToken> stringToken)
        {
            return new LogString(stringToken.StringWithoutQuotes);
        }
        
        [Production("scalar : Integer")]
        public LogSharpAST IntScalar(Token<LogSharpToken> intToken)
        {
            return new LogInt(intToken.IntValue);
        }
        
        [Production("scalar : Double")]
        public LogSharpAST DoubleScalar(Token<LogSharpToken> doubleToken)
        {
            return new LogDouble(doubleToken.DoubleValue);
        }*/
        
        
        
    }
}