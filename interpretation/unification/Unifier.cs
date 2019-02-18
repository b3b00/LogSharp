using System;
using System.Collections.Generic;
using logSharp.model;

namespace logSharp.interpretation.unification
{
    public class Unifier
    {

        public Unification UnifyTerms(Term source, TermEnvironment sourceEnv, Term destination,
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
                Valuable srcVal = null;
                Valuable destVal = null;
               
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



        public Valuable GetValue(Valuable arg, TermEnvironment environment)
        {
            Valuable val = null;
            if (arg.IsVariable)
            {
                val = environment[(arg as Variable).Name];
            }
            else
            {
                val = arg;
            }

            return val;
        }
    }
}