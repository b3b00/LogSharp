using System.Collections.Generic;
using System.Linq;
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
                var destArg = destination.Parameters[i];

                // resolve both sides while keeping the unbound variables visible
                // (placeholders) : terms can be partially instantiated
                Valuable srcVal = Eval(srcArg, sourceEnv, true);

                if (srcVal == null)
                {
                    return Unification.Failed();
                }

                Valuable destVal = Eval(destArg, unification, true);

                if (destVal is Variable destPlaceholder)
                {
                    // the destination side resolves to a still-unbound variable
                    // (possibly through aliases) : it must receive the source value
                    if (destArg is Variable && destArg.IsDiscardableVariable)
                    {
                        continue;
                    }

                    if (srcVal is Variable srcAlias)
                    {
                        // alias : the destination variable views the source variable
                        // (same variable on both sides : nothing to bind)
                        if (srcAlias.Name != destPlaceholder.Name)
                        {
                            unification.SetBinding(destPlaceholder.Name, srcAlias);
                        }
                    }
                    else
                    {
                        unification.SetBinding(destPlaceholder.Name, srcVal);
                    }

                    continue;
                }

                if (srcVal is Variable srcVariable)
                {
                    // the source side resolves to a still-unbound variable : bind it
                    // to the destination value (which is concrete at this point)
                    if (srcVariable.IsDiscardableVariable)
                    {
                        // the anonymous variable matches anything, without binding
                        continue;
                    }

                    if (destVal != null)
                    {
                        unification.SetBinding(srcVariable.Name, destVal);
                        continue;
                    }

                    return Unification.Failed();
                }

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
                    else
                    {
                        return Unification.Failed();
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
                    return Eval(environment.Environment[arg.Name],environment, preserveVariables);
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
                    var p = Eval(param, environment, preserveVariables);
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