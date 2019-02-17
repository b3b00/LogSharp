using System;
using System.Collections.Generic;
using logSharp.model;
using System.Text;

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
        
        public Unification(bool ok)
        {
            Environment = new TermEnvironment();
            IsOk = ok;
        }

        public void AddBinding(string variableName, LogValuable value)
        {
            Environment[variableName] = value;
        }

        public string ToString()
        {
            StringBuilder dump = new StringBuilder();
            if (IsOk)
            {
                dump.AppendLine("Yes !");
                dump.AppendLine(Environment.ToString());

                return dump.ToString();
            }
            else
            {
                dump.AppendLine("No.");
            }

            return dump.ToString();
        }
    }
    
    public class Unifier
    {

        public Unification UnifyTerms(LogTerm source, TermEnvironment sourceEnv, LogTerm destination,
            TermEnvironment destEnv)
        {
            if (source == null || destination == null)
            {
                return Unification.Failed();
            }

            Unification unification = new Unification();
            unification.Environment = destEnv;
            if (source.Name != destination.Name || source.Parameters.Count != destination.Parameters.Count)
            {
                unification = Unification.Failed();
            }

            for (int i = 0; i < source.Parameters.Count; i++)
            {
                var srcArg = source.Parameters[i];
                var destArg = destination.Parameters[i];
                LogValuable srcVal = null;
                LogValuable destVal = null;
               
                srcVal = GetValue(srcArg,sourceEnv);

                if (srcVal != null)
                {
                    destVal = GetValue(destArg, destEnv);

                    if (destVal == null)
                    {
                        unification.AddBinding(destArg.Name,srcVal);
                    }
                    else
                    {
                        if (destVal != srcArg)
                        {
                            return Unification.Failed();
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