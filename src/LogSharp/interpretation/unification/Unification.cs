using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

        public Unification(bool ok)
        {
            Environment = new TermEnvironment();
            IsOk = ok;
        }

        public void SetBinding(Variable variable, Valuable value)
        {
            Environment[variable.Name] = value;
        }

        public void SetBinding(string variableName, Valuable value)
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

        public bool HasBinding(string variableName)
        {
            return Environment.Environment.ContainsKey(variableName);
        }

        public Valuable GetBinding(string variableName)
        {
            if (Environment.Environment.ContainsKey(variableName))
            {
                return Environment.Environment[variableName];
            }

            return null;
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
            }
            else
            {
                dump.AppendLine("don't know.");
            }

            return dump.ToString();
        }

        [ExcludeFromCodeCoverage]
        public override bool Equals(object obj)
        {
            if (obj != null)
            {
                if (obj is Unification unification)
                {
                    var dico1 = Environment.Environment;
                    var dico2 = unification.Environment.Environment;
                    int count = dico1.Count; 
                    if (count == dico2.Count)
                    {
                        var keys1 = dico1.Keys.ToList();
                        var keys2 = dico2.Keys.ToList();
                        var intersection = keys1.Intersect(keys2);
                        if (intersection.Count() == count)
                        {
                            foreach (var pair in dico1)
                            {
                                if (dico2.ContainsKey(pair.Key))
                                {
                                    var value = dico2[pair.Key];
                                    if (!value.Equals(pair.Value))
                                    {
                                        return false;
                                    }
                                }
                            }

                            return true;
                        }
                    }
                }
            }

            return false;
        }

        [ExcludeFromCodeCoverage]
        public override int GetHashCode()
        {
            return Environment.GetHashCode();
        }
    }
}
