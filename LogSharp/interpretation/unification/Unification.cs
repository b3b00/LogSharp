using System.Linq;
using System.Text;
using logSharp.model;

namespace logSharp.interpretation.unification
{
    public class Unification
    {
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
            foreach (var binding in unification.Environment.Environment) Environment[binding.Key] = binding.Value;
        }

        public override string ToString()
        {
            var dump = new StringBuilder();
            if (IsOk)
            {
                dump.AppendLine("yes.");
                if (Environment.Environment.Any())
                {
                    var envStr = Environment.ToString();
                    dump.Append(envStr.Trim());
                }

                return dump.ToString();
            }

            dump.AppendLine("don't know.");

            return dump.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj != null)
                if (obj is Unification unification)
                {
                    var dico1 = Environment.Environment;
                    var dico2 = unification.Environment.Environment;
                    var count = dico1.Count;
                    if (count == dico2.Count)
                    {
                        var keys1 = dico1.Keys.ToList();
                        var keys2 = dico2.Keys.ToList();
                        var intersection = keys1.Intersect(keys2);
                        if (intersection.Count() == count)
                        {
                            foreach (var pair in dico1)
                                if (dico2.ContainsKey(pair.Key))
                                {
                                    var value = dico2[pair.Key];
                                    if (!value.Equals(pair.Value)) return false;
                                }

                            return true;
                        }
                    }
                }

            return false;
        }

        public override int GetHashCode()
        {
            return Environment.GetHashCode();
        }
    }
}