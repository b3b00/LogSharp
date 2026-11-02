using System;
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
            Console.WriteLine("=== MemberThirdTest starting ===");
            var answers = GetAnswers("member_of(c,[a,b,c])?");
            Console.WriteLine($"=== Number of answers: {answers.Count} ===");
            foreach (var a in answers)
            {
                Console.WriteLine($"=== Answer IsOk: {a.IsOk} ===");
                Console.WriteLine($"=== Answer: {a} ===");
            }
            Assert.Single(answers);
            Assert.True(answers[0].IsOk);
        }
    }
}