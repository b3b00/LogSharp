
namespace logSharp.model
{
    public class Question : Term
    {
        public Term ParentTerm { get; set; }
        public Question(Term term) : base(term.Name, term.Parameters)
        {
            ParentTerm = term;
        }
    }
}