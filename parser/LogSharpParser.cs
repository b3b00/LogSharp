using sly.parser.generator;
using System.Collections.Generic;
using System.Linq;
using LogSharp.model;
using sly.lexer;
using sly.parser.parser;

namespace LogSharp.parser
{
    public class LogSharpParser
    {

        [Production("program : rule *")]
        public LogSharpAST program(List<LogRule> rules)
        {
            return new LogProgram(rules);
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

        [Production("predicate : clause Imply[d] clause* Dot")]
        public LogSharpAST Predicate(LogClause head, List<LogClause> clauses)
        {
            return new LogPredicate(head as LogClause, clauses as List<LogClause>);
        }


        [Production("fact : clause Dot[d]")]
        public LogSharpAST fact(LogSharpAST clause)
        {
            return new LogFact(clause as LogClause);
        }


        [Production("clause : Atom LeftPar[d] (scalar Comma[d])* RightPar[d]")]
        public LogSharpAST clause(Token<LogSharpToken> clauseName, List<Group<LogSharpToken, LogSharpAST>> parameters)
        {
            return new LogClause(
                clauseName.StringWithoutQuotes,
                parameters
                    .Select((Group<LogSharpToken, LogSharpAST> group) => group.Value(0) as LogScalar)
                    .ToList<LogScalar>()
                );
        }

        [Production("scalar : Atom")]
        public LogSharpAST AtomScalar(Token<LogSharpToken> atom)
        {
            return new LogAtom(atom.StringWithoutQuotes);
        }
        [Production("scalar : String")]
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
        }
        
        
        
    }
}