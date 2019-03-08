using System;
using logSharp.model;
using Xunit;

namespace ogsharpTests
{
    public class IsExpressionsTests : BaseTest
    {
        public const string PrologProgram = @"

plusone(X,Y) :- is(X,Y + 1).
cstexpr(X) :- is(X,1 + 1). 
complexexpr(R,X,Y,Z) :- is(R,X + Y * Z).
"; 
        
        // core builtins
        // builtins discovery
        public IsExpressionsTests() : base(PrologProgram)
        {
        }
        
        [Fact]
        public void ConstantExprTest()
        {
            var answers = GetAnswers("cstexpr(A)?");
            Assert.NotNull(answers);
            Assert.Single(answers);
            var answer = answers[0];
            Assert.True(answer.Environment.ContainsVariable("A"));
            var a = answer.Environment["A"];
            var logInt = a as LogInt;
            Assert.NotNull(logInt);
            Assert.Equal(2,logInt.Value);
        }

        [Fact]
        public void PlusOneTest()
        {
            var answers = GetAnswers("plusone(A,41)?");
            Assert.NotNull(answers);
            Assert.Single(answers);
            var answer = answers[0];
            Assert.True(answer.Environment.ContainsVariable("A"));
            var a = answer.Environment["A"];
            var logInt = a as LogInt;
            Assert.NotNull(logInt);
            Assert.Equal(42,logInt.Value);
        }
        
        [Fact]
        public void ComplexTest()
        {
            var answers = GetAnswers("complexexpr(A,100,2,6)?");
            Assert.NotNull(answers);
            Assert.Single(answers);
            var answer = answers[0];
            Assert.True(answer.Environment.ContainsVariable("A"));
            var a = answer.Environment["A"];
            var logInt = a as LogInt;
            Assert.NotNull(logInt);
            Assert.Equal(112,logInt.Value);
        }
        
    }
}