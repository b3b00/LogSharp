using System;
using System.Collections.Generic;
using logSharp.model;

namespace logSharp.interpretation.unification
{
    public class Unifier : IUnifier
    {

        public Unification UnifyTerms(Term source, Unification sourceEnv, Term destination,
            Unification destEnv)
        {
            if (source == null || destination == null)
            {
                return Unification.Failed();
            }

            Unification unification = new Unification();
            unification.Environment = destEnv.Environment; // todo : deepcopy ?
            if (source.Name != destination.Name || source.Parameters.Count != destination.Parameters.Count)
            {
                unification = Unification.Failed();
            }

            bool ok = true;
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
                        if (!destVal.Equals(srcVal))
                        {
                            return Unification.Failed();
                        }
                    }
                }
            }

            unification.IsOk = true;
            return unification;
        }



        public Valuable GetValue(Valuable arg, Unification environment)
        {
            Valuable val = null;
            if (arg.IsVariable)
            {
                val = environment.Environment[(arg as Variable).Name]; // TODO : check if contained
            }
            else
            {
                val = arg;
            }

            return val;
        }
    }
}