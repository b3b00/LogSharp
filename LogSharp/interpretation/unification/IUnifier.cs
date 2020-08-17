using logSharp.model;

namespace logSharp.interpretation.unification
{
    public interface IUnifier
    {
        Unification UnifyTerms(Term source, Unification sourceEnv, Term destination,
            Unification destEnv);

        Term EvalTerm(Term term, Unification unification);
        Rule EvalRule(Rule rule, Unification unification);
    }
}