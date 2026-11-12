using NFluent;
using Xunit;
using Xunit.Abstractions;

namespace LogsharpTests;

public class CutTests : BaseTest
{
    public const string PrologProgram = @"
grade(Score, ""A"") :- Score >= 90 cut().
grade(Score, ""B"") :- Score >= 75 !.
grade(Score, ""C"") :- Score >= 60 !.
grade(Score, ""F"") :- Score < 60.

no(T) :- call(T) ! fail().
no(T).

equals(X,Y) :- X == Y.


";

    public CutTests() : base(PrologProgram)
    {
        
    }

    [Fact]
    public void TestGrade()
    {
        var answers = GetAnswers("grade(92,G)?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0]).Contains("G","A");

        answers = GetAnswers("grade(78,G)?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0]).Contains("G","B");
        
        answers = GetAnswers("grade(69,G)?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0]).Contains("G","C");
        
        answers = GetAnswers("grade(1,G)?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0]).Contains("G","F");
        
    }
    
    [Fact]
    public void TestNoWithCut()
    {
        var answers = GetAnswers("no(equals(42,42))?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0]).Not.IsOk();
        
        answers = GetAnswers("no(equals(42,24))?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0]).IsOk();

    }
}
