

using Xunit;

namespace logsharpTests
{
    public class TermTests : BaseTest
    {
        
        public const string PrologProgram = @"
is_in(X,l(X,_)).
is_in(X,l(_,L)) :- is_in(X,L).";


        public TermTests() : base(PrologProgram)
        {
        }
        
        [Fact]
        public void IsInSingle()
        {
            var answers = GetAnswers("is_in(a,l(a,())?");
            Assert.Single(answers);
            Assert.True(answers[0].IsOk);
        }

        [Fact]
        public void IsInFirstTest()
        {
            var answers = GetAnswers("is_in(a,l(a,l(b,c))?");
            Assert.Single(answers);
            Assert.True(answers[0].IsOk);
        }
        
        [Fact]
        public void MemberSecondTest()
        {
            var answers = GetAnswers("is_in(b,l(a,l(b,c))?");
            Assert.Single(answers);
            Assert.True(answers[0].IsOk);
        }
        
        [Fact]
        public void MemberThirdTest()
        {
            var answers = GetAnswers("is_in(c,l(a,l(b,c))?");
            Assert.Single(answers);
            Assert.True(answers[0].IsOk);
        }
    }
}