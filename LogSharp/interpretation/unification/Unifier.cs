using DeepCopy;
using logSharp.model;

namespace logSharp.interpretation.unification
{
    public class Unifier : IUnifier
    {
        public Unification UnifyTerms(Term source, Unification sourceEnv, Term destination,
            Unification destEnv)
        {
            if (source == null || destination == null) return Unification.Failed();

            var unification = new Unification();
            unification.Environment = DeepCopier.Copy(destEnv.Environment);
            if (source.Name != destination.Name || source.Parameters.Count != destination.Parameters.Count)
                unification = Unification.Failed();

            for (var i = 0; i < source.Parameters.Count; i++)
            {
                var srcArg = source.Parameters[i];
                var destArg = destination.Parameters[i];
                Valuable srcVal = null;
                Valuable destVal = null;

                srcVal = GetValue(srcArg, sourceEnv);

                if (srcVal != null)
                {
                    destVal = GetValue(destArg, destEnv);

                    if (destVal == null)
                    {
                        unification.AddBinding(destArg.Name, srcVal);
                    }
                    else
                    {
                        if (!destVal.Equals(srcVal)) return Unification.Failed();
                    }
                }
            }

            unification.IsOk = true;
            return unification;
        }

        public Term EvalTerm(Term term, Unification unification)
        {
            return term;
        }


        public Valuable GetValue(Valuable arg, Unification environment)
        {
            Valuable val = null;
            if (arg.IsVariable)
            {
                var variable = arg as Variable;
                if (environment.Environment.ContainsVariable(variable.Name))
                    val = environment.Environment[variable.Name];
                else
                    val = variable;
            }
            else
            {
                val = arg;
            }

            return val;
        }
    }
}