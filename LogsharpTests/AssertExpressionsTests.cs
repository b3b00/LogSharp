using System;
using logSharp.model;
using Xunit;

namespace logsharpTests
{
    public class AssertExpressionsTests : BaseTest
    {
        private const string PrologProgram = @"
            gt(X,Y) :- X > Y.     
            lte(X,Y) :- X <= Y.
            eq(X,Y) :- X == Y.
            diff(X,Y) :- X != Y.
            gtc(X,Y) :- X + Y > X * Y. 
"; 
        
        
        public AssertExpressionsTests() : base(PrologProgram)
        {
        }
        

        private void AssertBaseTest(string query, bool expected)
        {
            var answers = GetAnswers(query);
            Assert.NotNull(answers);
            Assert.Single(answers);
            var answer = answers[0];
            Assert.True(answer.IsOk == expected,$"{query} expecting {expected}, actual {answer.IsOk}");
        }

        [Fact]
        public void TestGt()
        {
            AssertBaseTest("gt(1, 2)?",false);
        }
        
        [Fact]
        public void TestLte()
        {
            AssertBaseTest("lte(2 , 2)?",true);
        }
        
        [Fact]
        public void TestEq()
        {
            AssertBaseTest("eq(3,3)?",true);
        }
        
        [Fact]
        public void TestDiff()
        {
            AssertBaseTest("diff(48,12)?",true);
        }
        
        [Fact]
        public void TestNotEq()
        {
            AssertBaseTest("eq(48,12)?",false);
        }

        [Fact]
        public void TestGtComplex()
        {
            AssertBaseTest("gtc(2,1)?", true);
        }


    }
}