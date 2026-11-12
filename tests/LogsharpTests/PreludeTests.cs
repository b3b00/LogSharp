using NFluent;
using Xunit;

namespace LogsharpTests;

public class PreludeTests : BaseTest
{
    public const string PrologProgram = @"

";

    public PreludeTests() : base(PrologProgram)
    {
        
    }

    [Fact]
    public void LenToVariableTest()
    {
        var answers = GetAnswers("len([a,b,c],L)?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0]).IsNotNull();
        Check.That(answers[0]).IsOk();
        var unification = answers[0];
        Check.That(unification.HasBinding("L")).IsTrue();
        var value =  unification.GetBinding("L");
        Check.That(value).IsNotNull();
        Check.That(value.ToString()).IsEqualTo("3");
    }
    
    [Fact]
    public void CheckLenTest()
    {
        var answers = GetAnswers("len([a,b,c],3)?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0]).IsNotNull();
        Check.That(answers[0]).IsOk();
    }
    
}