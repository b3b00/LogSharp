using System;
using Xunit;

namespace ogsharpTests
{
    public class IsExpressionsTests : BaseTest
    {
        public const string PrologProgram = @"

plusOne(X,Y) :- is(Y,""X + 1""). 
"; 
        
        // core builtins
        // builtins discovery
        public IsExpressionsTests() : base(PrologProgram)
        {
        }

        [Fact]
        public void PlusOneTest()
        {
            var answers = GetAnswers("plusOne(A,5)?");
            Assert.NotNull(answers);
            Assert.Equal(7, answers.Count);
        }
        
    }
}