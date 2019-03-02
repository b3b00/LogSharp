using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeepCopy;
using logSharp.interpretation.unification;
using logSharp.model;

namespace logSharp.interpretation
{
    public class Goal
    {
        public static int NextGoalId;

        public List<Goal> Goals = new List<Goal>();

        public Term Head;

        public Goal(Question question)
        {
            Parent = null;
            Head = question;
            Goals = new List<Goal>();
            Unification = new Unification();
            Unification.IsOk = false;
            Index = 0;
        }

        public Goal(Rule rule, Goal parent, Unification environment)
        {
            Rule = rule;
            Parent = parent;
            if (Unification != null)
                Unification.AddBindings(environment);
            else
                Unification = DeepCopier.Copy(environment);
            Goals = new List<Goal>();

            Head = rule.Head;
            if (rule.Clauses != null && rule.Clauses.Any())
            {
                foreach (var t in rule.Clauses)
                    if (t != null)
                        Goals.Add(new Goal(new Question(t)));
                    else
                        ;
            }
            else
            {
                Goals.Add(new Goal(new Question(rule.Head)));
                ;
            }

            Index = 0;
            Id = NextGoalId;
            NextGoalId++;
        }

        public int Id { get; }

        public Rule Rule { get; set; }

        public Goal Parent { get; set; }

        public bool IsRootGoal => Parent == null;

        public int Index { get; set; }

        public Unification Unification { get; set; }


        public int Count => Goals.Count;

        public bool IsOk => Unification.IsOk;


        public bool IsFact => Rule != null && Rule.IsFact;

        public bool IsPredicate => Rule != null && Rule.IsPredicate;

        public string Specification => Head.Specification;


        public override string ToString()
        {
            var dump = new StringBuilder();
            if (Rule != null)
                dump.AppendLine(Rule + " @ " + Index);
            else
                dump.AppendLine(Head + " @ " + Index);

            var unifStr = Unification.ToString();
            dump.AppendLine(unifStr);
            return dump.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj != null)
                if (obj is Goal goal)
                {
                    var termEquals = goal.Head.Equals(Head);
                    var unifEqual = Unification.Equals(goal.Unification);
                    return termEquals && unifEqual;
                }

            return false;
        }

        public override int GetHashCode()
        {
            return Head.GetHashCode() + Unification.GetHashCode();
        }
    }
}