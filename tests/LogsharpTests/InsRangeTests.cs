using System.Linq;
using NFluent;
using Xunit;

namespace LogsharpTests;

public class InsRangeTests : BaseTest
{
    private const string Program = @"
% the bounds of the ins operator may be integer literals, variables or
% arithmetic expressions
range_test(Vs) :-
    length(Vs, 3)
    Min is 1
    Max is 2 + 1
    Vs ins Min...Max
    label(Vs).

literal_test(Vs) :-
    length(Vs, 2)
    Vs ins 1...2
    label(Vs).

crossed_test(Vs) :-
    length(Vs, 2)
    Vs ins 2...1.

expression_bound_test(Vs) :-
    length(Vs, 2)
    Vs ins 1...2 + 1
    label(Vs).
";

    public InsRangeTests() : base(Program)
    {
    }

    [Fact]
    public void VariableAndExpressionBoundsAreAccepted()
    {
        // Min = 1 and Max = 2 + 1 = 3 : 3 free variables, 3 values each
        var answers = GetAnswers("range_test(Vs)?");
        Check.That(answers).CountIs(27);
        Check.That(answers.All(answer => answer.IsOk)).IsTrue();
    }

    [Fact]
    public void LiteralBoundsStillWork()
    {
        var answers = GetAnswers("literal_test(Vs)?");
        Check.That(answers).CountIs(4);
        Check.That(answers.All(answer => answer.IsOk)).IsTrue();
    }

    [Fact]
    public void ArithmeticBoundExpressionIsEvaluated()
    {
        // the upper bound 2 + 1 evaluates to 3 : 2 free variables, 3 values each
        var answers = GetAnswers("expression_bound_test(Vs)?");
        Check.That(answers).CountIs(9);
        Check.That(answers.All(answer => answer.IsOk)).IsTrue();
    }

    [Fact]
    public void CrossedBoundsFail()
    {
        var answers = GetAnswers("crossed_test(Vs)?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0].IsOk).IsFalse();
    }
}
