using sly.parser.generator;
using System.Collections.Generic;
using System.Linq;
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

        [Production("predicate : clause Imply[d] clause* Dot[d]")]
        public LogSharpAST Predicate(LogSharpAST head, List<LogSharpAST> clauses)
        {
            return new LogPredicate(head as LogClause, clauses.Cast<LogClause>().ToList());
        }


        [Production("fact : clause Dot[d]")]
        public LogSharpAST fact(LogSharpAST clause)
        {
            return new LogFact(clause as LogClause);
        }
        
        [Production("fact : Atom Dot[d]")]
        public LogSharpAST fact(Token<LogSharpToken> atom)
        {
            return new LogFact(atom.Value);
        }



        [Production("clause : Atom LeftPar[d] scalar (Comma[d] scalar)*  RightPar[d]")]
        public LogSharpAST clause(Token<LogSharpToken> clauseName, LogSharpAST headParam, List<Group<LogSharpToken, LogSharpAST>> tailParams)
        {
            List<LogScalar> parameters = new List<LogScalar>();
            parameters.Add(headParam as LogScalar);
            foreach (var paramGroup in tailParams)
            {
                LogScalar ast = paramGroup.Value(0) as LogScalar;
                parameters.Add(ast);
            }

            return new LogClause(
                clauseName.StringWithoutQuotes,
                parameters
                );
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