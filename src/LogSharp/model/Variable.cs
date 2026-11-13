using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using logSharp.interpretation.unification;

namespace logSharp.model
{
    public class Variable : Valuable, IExpression
    {

        public const string DiscardingVariable = "_";
        
        public bool IsScalar => false;

        public bool IsVariable => true;
        
        public bool IsDiscardableVariable => Name == DiscardingVariable;

        public bool IsTerm => false;
        
        public bool IsList => false;
        
        public bool IsConstantList => true;
        public string Name { get; set; }

        public Variable(string name)
        {
            Name = name;
        }

        public List<Variable> GetFreeVariables() => new List<Variable>() { this };
        
        public object Evaluate(Unification unification)
        {
            if (unification.Environment.ContainsVariable(Name))
            {
                var logValue = unification.Environment[Name];
                if (logValue is Scalar scalar)
                {
                    return scalar.RawValue;
                }
            }

            return null;
        }
        
        [ExcludeFromCodeCoverage]
        public string Dump()
        {
            return Name;
        }

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return Dump();
        }
    }
}