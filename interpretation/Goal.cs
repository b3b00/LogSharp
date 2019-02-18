using System.Collections.Generic;
using System.Text;
using DeepCopy;
using logSharp.interpretation.unification;
using logSharp.model;

namespace logSharp.interpretation
{
    public class Goal
    {
        public static int NextGoalId = 0;
        
        public int Id { get; private set; }
        
        public Rule Rule { get; set; }
        
        public Goal Parent { get; set; }

        public bool IsRootGoal => Parent == null;
        
        public List<Term> Clauses = new List<Term>();

        public Term Head;

        public int Index { get; set; } 
        
        public Unification Unification { get; set; }

        public int Count => Clauses.Count;

        public bool IsOk => Unification.IsOk;

        public Goal(Question question)
        {
            Parent = null;
            Head = question;
            Clauses = new List<Term>();
            Unification = new Unification();
            Index = 0;
        }

        public Goal(Rule rule, Goal parent, Unification environment)
        {
            Rule = rule;
            Parent = parent;
            Unification = DeepCopier.Copy(environment);

            Head = rule.Head;
            Clauses = rule.Clauses;

            Index = 0;
            Id = NextGoalId;
            NextGoalId++;
        }



        public override string ToString()
        {
            StringBuilder dump = new StringBuilder();
            dump.AppendLine(base.ToString());
            dump.AppendLine(Unification.ToString());
            return dump.ToString();
        }
        
    }
}