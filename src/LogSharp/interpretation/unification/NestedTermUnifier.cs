using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using DeepCopy;
using logSharp.model;

namespace logSharp.interpretation.unification
{
    public class NestedTermUnifier : IUnifier
    {

        public Unification UnifyTerms(Term source, Unification sourceEnv, Term destination,
            Unification destEnv)
        {
            if (source == null || destination == null)
            {
                return Unification.Failed();
            }

            if (source.Name != destination.Name || source.Parameters.Count != destination.Parameters.Count)
            {
                return Unification.Failed();
            }

            Unification unification = DeepCopier.Copy(destEnv);
            unification.Environment = DeepCopier.Copy(destEnv.Environment); 

            for (int i = 0; i < source.Parameters.Count; i++)
            {
                var srcArg = source.Parameters[i];
                if (destination.Parameters.Any() || i < destination.Parameters.Count)
                {

                    var destArg = destination.Parameters[i];
                    Valuable srcVal = null;
                    Valuable destVal = null;

                    srcVal = Eval(srcArg, sourceEnv);

                    if (srcVal != null)
                    {
                        destVal = Eval(destArg, unification);

                        if (destVal == null && destArg.IsVariable)
                        {
                            if (destArg.Name != Variable.DiscardingVariable)
                            {
                                unification.AddBinding(destArg as Variable, srcVal);
                            }
                            else
                            {
                                ;
                            }
                        }
                        else
                        {
                            if (srcVal is Scalar)
                            {
                                if (destVal is Scalar)
                                {
                                    if (!destVal.Equals(srcVal))
                                    {
                                        return Unification.Failed();
                                    }
                                }
                                else
                                {
                                    return Unification.Failed();
                                }
                            }
                            else if (srcVal is Term srcTerm)
                            {
                                if (destVal != null && destVal is Term destValTerm)
                                {
                                    var u = UnifyTerms(srcTerm, sourceEnv, destValTerm, unification);
                                    if (u.IsOk)
                                    {
                                        unification.AddBindings(u);
                                    }
                                    else
                                    {
                                        return Unification.Failed();
                                    }
                                }
                                else if (destVal == null && destArg is Term destArgTerm)
                                {
                                    var u = UnifyTerms(srcTerm, sourceEnv, destArgTerm, unification);
                                    if (u.IsOk)
                                    {
                                        unification.AddBindings(u);
                                    }
                                    else
                                    {
                                        return Unification.Failed();
                                    }
                                }
                                else
                                {
                                    return Unification.Failed();
                                }
                            }
                            else
                            {
                                ;
                            }

                        }
                    }
                }
            }

            unification.IsOk = true;
            return unification;
        }


        public Term EvalTerm(Term term, Unification unification)
        {
            return Eval(term as Valuable, unification, true) as Term;
        }
      

        public Valuable Eval(Valuable arg, Unification environment,bool preserveVariables = false)
        {
            if (arg.IsScalar)
            {
                return arg;
            }
            if (arg.IsVariable)
            {
                if (environment.Environment.ContainsVariable(arg.Name))
                {
                    return Eval(environment.Environment[arg.Name],environment);
                }
                else
                {
                    if (preserveVariables)
                    {
                        return arg;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            else
            {
                Term term = arg as Term;
                var newParameters = new List<Valuable>();
                foreach (var param in term.Parameters)
                {
                    var p = Eval(param, environment);
                    if (p != null)
                    {
                        newParameters.Add(p);
                    }
                    else
                    {
                        if (preserveVariables || param.IsDiscardableVariable)
                        {
                            newParameters.Add(param);
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                return new Term(term.Name,newParameters);
            }
        }
    }
}