using System.Collections.Generic;
using System.Linq;
using logSharp.interpretation.unification;

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

        public virtual IEnumerable<Variable> FreeVariables =>
            Head.FreeVariables.Concat(Clauses.SelectMany(c => c.FreeVariables));
        
        public virtual bool IsFullyUnified(Unification unification)
        {
            var frees = FreeVariables;
            return  frees.Select(x => x.IsDiscardableVariable || unification.Environment.ContainsVariable(x.Name))
                .Aggregate((bool b1, bool b2) => b1 && b2);;
        }
        
        public abstract string Dump();
    }
}