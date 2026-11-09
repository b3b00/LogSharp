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

        
        public int Depth => IsRootGoal ? 0 : Parent?.Depth ?? 0;
        
        public List<Goal> Goals = new List<Goal>();

        public Term Head;

        public int Index { get; set; } 

        public bool IsContinuation { get; set; }
        
        public Unification Unification { get; set; }
        

        public int Count => Goals.Count;

        public bool IsOk => Unification.IsOk;


        public bool IsFact => Rule != null && Rule.IsFact;
        
        public bool IsPredicate => this.Rule != null && Rule.IsPredicate;

        public string Specification => Head.Specification;

        private List<Goal> _children = new List<Goal>();
        public List<Goal> Children => _children;

        private bool _isCut = false;
        public bool IsCut => _isCut;
        
        public Goal(Question question)
        {
            Parent = null;
            Head = question;
            Goals = new List<Goal>();
            Unification = new Unification();
            Unification.IsOk = false;
            Index = 0;
        }

        public List<Variable> GetFreeVariables() => Head.GetFreeVariables();

        public void AddChild(Goal child)
        {
            _children.Add(child);
        }

        public void Cut()
        {
            _isCut = true;
        }

        public Goal(Rule rule, Goal parent, Unification environment, bool freshenVariables = true)
        {
            IsContinuation = !freshenVariables;
            Id = NextGoalId;
            NextGoalId++;
            Parent = parent;
            if (Unification != null)
            {
                Unification.AddBindings(environment);
            }
            else
            {
                Unification = DeepCopier.Copy(environment);
            }
            Rule = freshenVariables ? FreshenRule(rule, Id) : rule;
            Goals = new List<Goal>();

            Head = Rule.Head;
            if (Rule.Clauses != null && Rule.Clauses.Any())
            {
                foreach (var t in Rule.Clauses)
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
        }

        private static Rule FreshenRule(Rule rule, int invocationId)
        {
            var variables = new Dictionary<string, Variable>();
            var head = FreshenTerm(rule.Head, variables, invocationId);
            if (rule.Clauses == null)
            {
                return new Fact(head);
            }

            var clauses = rule.Clauses
                .Select(clause => FreshenTerm(clause, variables, invocationId))
                .ToList();
            return new Predicate(head, clauses);
        }

        private static Term FreshenTerm(Term term, Dictionary<string, Variable> variables, int invocationId)
        {
            var parameters = term.Parameters.Select(parameter => FreshenValue(parameter, variables, invocationId)).ToList();
            return new Term(term.Name, parameters);
        }

        private static Valuable FreshenValue(Valuable value, Dictionary<string, Variable> variables, int invocationId)
        {
            if (value is Variable variable)
            {
                if (variable.IsDiscardableVariable)
                {
                    return variable;
                }

                if (!variables.TryGetValue(variable.Name, out var freshVariable))
                {
                    freshVariable = new Variable($"__goal{invocationId}_{variable.Name}");
                    variables.Add(variable.Name, freshVariable);
                }

                return freshVariable;
            }

            if (value is Term term)
            {
                return FreshenTerm(term, variables, invocationId);
            }
            
            // TODO expressions
            if (value is IExpression expression)
            {
                
                return FreshenExpression(expression, variables, invocationId);
            }

            return value;
        }

        private static Valuable FreshenExpression(IExpression expression, Dictionary<string, Variable> variables, int invocationId)
        {
            // TODO
            return expression switch
            {
                BinaryOperation binary => new BinaryOperation(
                    FreshenValue(binary.LeftExpresion as Valuable, variables, invocationId),
                    binary.Operator,
                    FreshenValue(binary.RightExpression as Valuable, variables, invocationId)
                ),
                UnaryOperation unary => new UnaryOperation(
                    unary.Operator,
                    FreshenValue(unary.Expression as Valuable, variables, invocationId)
                ),
                Variable variable => variables[variable.Name],
                _ => expression
            };
        }


        public override string ToString()
        {
            StringBuilder dump = new StringBuilder();
            if (this.Rule != null)
            {
                dump.AppendLine(this.Rule.ToString() + " @ " + Index);
            }
            else
            {
                dump.AppendLine(Head.ToString() + " @ " + Index);
            }

            return dump.ToString();
        }
    }
}