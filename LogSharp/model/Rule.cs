using System.Collections.Generic;

namespace logSharp.model
{
    public abstract class Rule : ILogSharpAst
    {
        
        public Term Head { get; set; }
        
        public List<Term> Clauses { get; set; }

        public List<IValuable> Parameters => Head.Parameters;

        public bool IsFact => this is Fact;
        
        public bool IsPredicate => this is Predicate;

        public Rule(Term head)
        {
            Head = head;
        }
        
        public Rule(Term head, List<Term> clauses)
        {
            Head = head;
            Clauses = clauses;
        }

        public string Name => Head.Name;

        public int Arity => Head.Parameters.Count;

        public string Specification => $"{Name}/{Arity}";
        public abstract string Dump();
    }
}