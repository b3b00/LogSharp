namespace logSharp.model
{
    public class Question : Term
    {
        public Question(Term term) : base(term.Name, term.Parameters)
        {
        }
    }
}