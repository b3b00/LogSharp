using System.Collections.Generic;
using logSharp.interpretation.unification;
using NFluent;
using Xunit;

namespace ogsharpTests;

public class MathTests: BaseTest
{
    public MathTests() : base(PrologProgram)
    {
    }

    public const string PrologProgram = @"
plusOne(X,Y) :- Y is X + 1.
conditionalAdd(X,Y) :- X < 10 Y is X + 1.
conditionalAdd(X,Y) :- X >= 10 Y is X + 10.
complex(X,Y) :- Y is (X + 69) * 12 - 4.
";

    [Fact]
    public void TestAdd1()
    {
        var answers = GetAnswers("plusOne(1,Y)?");
        Check.That(answers).IsNotNull();
        Check.That(answers).CountIs(1);
        Check.That(answers[0].IsOk).IsTrue();
        Check.That((answers[0])).Contains("Y", "2");
    }
    
    [Fact]
    public void TestComplexExpression()
    {
        var answers = GetAnswers("complex(23,Y)?");
        Check.That(answers).IsNotNull();
        Check.That(answers).CountIs(1);
        Check.That(answers[0].IsOk).IsTrue();
        Check.That((answers[0])).Contains("Y", "1100");
    }
    
    [Fact]
    public void TestConditionalAdd()
    {
        var answers = GetAnswers("conditionalAdd(1,Y)?");
        Check.That(answers).Not.IsNullOrEmpty();
        Check.That(answers).CountIs(1);
        Check.That(answers[0].IsOk).IsTrue();
        Check.That((answers[0])).Contains("Y", "2");
        
        answers = GetAnswers("conditionalAdd(42,Y)?");
        Check.That(answers).Not.IsNullOrEmpty();
        Check.That(answers).CountIs(1);
        Check.That(answers[0].IsOk).IsTrue();
        Check.That((answers[0])).Contains("Y", "52");
        
        
        

    }
        
    
}