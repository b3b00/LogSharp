

using Xunit;

namespace LogsharpTests
{
    public class ListTests : BaseTest
    {
        
        public const string PrologProgram = @"
member_of(X,[X|_]).
member_of(X,[_|L]) :- member_of(X,L).";


        public ListTests() : base(PrologProgram)
        {
        }
        
        [Fact]
        public void MemberSingle()
        {
            var answers = GetAnswers("member_of(a,[a])?");
            Assert.Single(answers);
            Assert.True(answers[0].IsOk);
        }

        [Fact]
        public void MemberFirstTest()
        {
            var answers = GetAnswers("member_of(a,[a,b,c])?");
            Assert.Single(answers);
            Assert.True(answers[0].IsOk);
        }
        
        [Fact]
        public void MemberSecondTest()
        {
            var answers = GetAnswers("member_of(b,[a,b,c])?");
            Assert.Single(answers);
            Assert.True(answers[0].IsOk);
        }
        
        [Fact]
        public void MemberThirdTest()
        {
            var answers = GetAnswers("member_of(c,[a,b,c])?");
            Assert.Single(answers);
            Assert.True(answers[0].IsOk);
        }
    }
}