using NFluent;
using Xunit;

namespace ogsharpTests;

public class MathTests: BaseTest
{
    public MathTests() : base(PrologProgram)
    {
    }

    public const string PrologProgram = @"plusOne(X,Y) :- Y is X + 1.";

    [Fact]
    public void TestIs()
    {
        var answers = GetAnswers("plusOne(1,Y)?");
        Check.That(answers).IsNotNull();
        Check.That(answers).CountIs(1);
        Check.That(answers[0].IsOk).IsTrue();

    }
        
    
}