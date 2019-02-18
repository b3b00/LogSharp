using System.Text;
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

        public void AddBinding(string variableName, Valuable value)
        {
            Environment[variableName] = value;
        }

        public override string ToString()
        {
            StringBuilder dump = new StringBuilder();
            if (IsOk)
            {
                dump.AppendLine("Yes.");
                string envStr = Environment.ToString();
                dump.Append(envStr.Trim());

                return dump.ToString();
            }
            else
            {
                dump.AppendLine("No.");
            }

            return dump.ToString();
        }
    }
}