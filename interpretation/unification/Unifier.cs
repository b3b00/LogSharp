using System;
using System.Collections.Generic;
using logSharp.model;

namespace logSharp.interpretation.unification
{

    public class Unification
    {
        public Dictionary<string,LogValuable> Environment { get; set; }
        
        public bool IsOk { get; set; }

        public static Unification Failed()
        {
            return new Unification(false);
        }

        public Unification()
        {
            Environment = new Dictionary<string, LogValuable>();
            IsOk = true;
        }
        
        public Unification(bool ok)
        {
            Environment = new Dictionary<string, LogValuable>();
            IsOk = ok;
        }

        public void AddBinding(string variableName, LogValuable value)
        {
            Environment[variableName] = value;
        }
    }
    
    public class Unifier
    {
        public Unification Unify(LogTerm source, TermEnvironment sourceEnv, LogTerm destination, TermEnvironment destEnv)
        {
            if (source == null || destination == null)
            {
                return Unification.Failed();
            }
            Unification unification = new Unification();
            unification.Environment = destEnv.Environment;
            if (source.Name != destination.Name || source.Parameters.Count != destination.Parameters.Count)
            {
                unification = Unification.Failed();
            }
            else
            {
                for (int i = 0; i < source.Parameters.Count; i++)
                {
                    var srcArg = source.Parameters[i];
                    var destArg = destination.Parameters[i];
                    var srcVal = GetValue(srcArg,sourceEnv);
                    if (srcVal != null)
                    {
                        var destVal = GetValue(destArg, destEnv);
                        
                        if (destVal == null)
                        {
                            unification.Environment[destArg.Name] = srcVal;
                        }
                        else if (destVal != srcVal)
                        {
                            unification = Unification.Failed();
                            break;
                        }
                    }
                    else
                    {
                        // both variable
                        if (destArg != srcArg)
                        {
                            unification = Unification.Failed();
                            break;
                        }
                    }
                }
            }

            return unification;
        }

        public LogValuable GetValue(LogValuable arg, TermEnvironment environment)
        {
            LogValuable val = null;
            if (arg.IsVariable)
            {
                val = environment[(arg as LogVariable).Name];
            }
            else
            {
                val = arg;
            }

            return val;
        }
    }
}