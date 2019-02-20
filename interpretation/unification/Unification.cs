using System.Text;
using System.Linq;
using logSharp.model;

namespace logSharp.interpretation.unification
{
    public class Unification
    {
        public TermEnvironment Environment { get; set; }
        
        public bool IsOk { get; set; }

        public static Unification Failed()
        {
            return new Unification(false);
        }
        
        public static Unification Success()
        {
            return new Unification(true);
        }

        public Unification()
        {
            Environment = new TermEnvironment();
            IsOk = true;
        }
        
        public Unification(TermEnvironment environment)
        {
            Environment = environment;
            IsOk = true;
        }
        
        public Unification(bool ok)
        {
            Environment = new TermEnvironment();
            IsOk = ok;
        }
        
        public void AddBinding(Variable variable, Valuable value)
        {
            Environment[variable.Name] = value;
        }

        public void AddBinding(string variableName, Valuable value)
        {
            Environment[variableName] = value;
        }
        public void AddBindings(Unification unification)
        {
            foreach (var binding in unification.Environment.Environment)
            {
                Environment[binding.Key] = binding.Value;
            }
        }

        public override string ToString()
        {
            StringBuilder dump = new StringBuilder();
            if (IsOk)
            {
                dump.AppendLine("yes.");
                if (Environment.Environment.Any())
                {
                    string envStr = Environment.ToString();
                    dump.Append(envStr.Trim());
                }
                return dump.ToString();
            }
            else
            {
                dump.AppendLine("don't know.");
            }

            return dump.ToString();
        }
    }
}
