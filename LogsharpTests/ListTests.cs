using System;
using System.Linq;
using Xunit;

namespace ogsharpTests
{
    public class ListTests : BaseTest
    {
        public ListTests() : base(PrologProgram)
        {
        }
        
        public const string PrologProgram = @"
member_of(X,[X|_]).
member_of(X,[_|L]) :- member_of(X,L).
";

        [Fact]
        public void MemberOfFirstTest()
        {
            var answers = GetAnswers("member_of(a,[a,b,c])?");
            Assert.Single(answers);
            Assert.True(answers[0].IsOk);
        }
        
        [Fact]
        public void MemberOfSecondTest()
        {
            var answers = GetAnswers("member_of(b,[a,b,c])?");
            Assert.Single(answers);
            Assert.True(answers[0].IsOk);
        }
        
        [Fact]
        public void MemberOfThirdTest()
        {
            var answers = GetAnswers("member_of(c,[a,b,c])?");
            Assert.Single(answers);
            Assert.True(answers[0].IsOk);
        }
        
        [Fact]
        public void MemberFirstTest()
        {
            var answers = GetAnswers("member(a,[a,b,c])?");
            Assert.Single(answers);
            Assert.True(answers[0].IsOk);
        }
        
        [Fact]
        public void MemberSecondTest()
        {
            var answers = GetAnswers("member(b,[a,b,c])?");
            Assert.Single(answers);
            Assert.True(answers[0].IsOk);
        }
        
        [Fact]
        public void MemberThirdTest()
        {
            var answers = GetAnswers("member(c,[a,b,c])?");
            Assert.Single(answers);
            Assert.True(answers[0].IsOk);
        }
    }
}