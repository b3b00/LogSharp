using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        
        public List<Goal> Goals = new List<Goal>();

        public Term Head;

        public int Index { get; set; } 
        
        public Unification Unification { get; set; }
        

        public int Count => Goals.Count;

        public bool IsOk => Unification.IsOk;

        public int Depth => Parent == null ? 0 : Parent.Depth + 1; 


        public bool IsFact => Rule != null && Rule.IsFact;
        
        public bool IsPredicate => Rule != null && Rule.IsPredicate;

        public string Specification => Head.Specification;

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
            {
                Unification.AddBindings(environment);
            }
            else
            {
                Unification = DeepCopier.Copy(environment);
            }
            Goals = new List<Goal>();

            Head = rule.Head;
            if (rule.Clauses != null && rule.Clauses.Any())
            {
                foreach (var t in rule.Clauses)
                {
                    if (t != null)
                    {
                        Goals.Add(new Goal(new Question(t)));
                    }
                    else
                    {
                        ;
                    }
                }
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



        public override string ToString()
        {
            StringBuilder dump = new StringBuilder();
            var tabs = new String('\t', Depth);
            if (Rule != null)
            {
                dump.AppendLine($"{tabs}Depth>{Depth}<  {Rule}  @ {Index} {Unification.Environment.ToString()}");
            }
            else
            {
                dump.AppendLine($"{tabs}Depth>{Depth}< {Head}  @ {Index} {Unification.Environment.ToString()}");
            }

            return dump.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj != null)
            {
                if (obj is Goal goal)
                {
                    bool termEquals = goal.Head.Equals(Head);
                    bool unifEqual = Unification.Equals(goal.Unification);
                    return termEquals && unifEqual;
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Head.GetHashCode() + Unification.GetHashCode();
        }


        private string Tab(int d)
        {
            string tab = "";
            for (int i = 0; i < d; i++)
            {
                tab += "\t";
            }

            return tab;
        }
        
        public string StackTrace()
        {
            StringBuilder builder = new StringBuilder();
            Goal current = this;
            while (current != null)
            {
                builder.Append(Tab(current.Depth))
                    .AppendLine(current.ToString());
                current = current.Parent;
            }

            return builder.ToString();
        }
    }
}