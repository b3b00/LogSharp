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

            Unification unification = new Unification();
            unification.Environment = DeepCopier.Copy(destEnv.Environment); // todo : deepcopy ?
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

                srcVal = Eval(srcArg, sourceEnv);

                if (srcVal != null)
                {
                    destVal = Eval(destArg, destEnv);

                    if (destVal == null && destArg.IsVariable)
                    {
                        if (destArg.Name != "_")
                        {
                            unification.AddBinding(destArg.Name, srcVal);
                        }
                        else
                        {
                            ;
                        }
                    }
                    else
                    {
                        // TODO : many cases => recurse in terms 
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
                                var u = UnifyTerms(srcTerm, sourceEnv, destValTerm, destEnv);
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
                                var u = UnifyTerms(srcTerm, sourceEnv, destArgTerm, destEnv);
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

        public Valuable Eval(Valuable arg, Unification environment)
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
                    return null;
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
                        return null;
                    }
                }
                return new Term(term.Name,newParameters);
            }
        }
    }
}