using logSharp.model;
using Xunit;

namespace ogsharpTests;

// temporary probe : DELETE ME
public class ScratchProbeTest : BaseTest
{
    public const string PrologProgram = @"
h([X|Rest], X).
third([_,_,Y|Rest], Y).
two_elems([A,B|Rest], A, B).
p(X) :- length(L,3) h(L,X).
q(Y) :- length(L,4) third(L,Y).
r(A,B) :- length(L,3) two_elems(L,A,B).
len3(L) :- length(L,3).
same_len(X,Y) :- length(X,N) length(Y,N).
samel(X) :- len3(L) same_len(L,X).
blocks([], [], []).
blocks([A,B,C|Bs1], [D,E,F|Bs2], [G,H,I|Bs3]) :- blocks(Bs1, Bs2, Bs3).
b1(R) :- length(R,9) blocks(R,R,R).
";

    public ScratchProbeTest() : base(PrologProgram)
    {
    }

    [Fact]
    public void ProbeHeadWithGeneratedVarList()
    {
        var answers = GetAnswers("p(X)?");
        Assert.Single(answers);
        Assert.True(answers[0].IsOk);
    }

    [Fact]
    public void ProbeThird()
    {
        var answers = GetAnswers("q(Y)?");
        Assert.Single(answers);
        Assert.True(answers[0].IsOk);
    }

    [Fact]
    public void ProbeTwoElems()
    {
        var answers = GetAnswers("r(A,B)?");
        Assert.Single(answers);
        Assert.True(answers[0].IsOk);
    }

    [Fact]
    public void ProbeSameLenGenerated()
    {
        var answers = GetAnswers("samel(X)?");
        Assert.Single(answers);
        Assert.True(answers[0].IsOk);
        var b = answers[0].GetBinding("X") as Term;
        Assert.NotNull(b);
        Assert.True(b.IsList);
        Assert.Equal(3, b.ToValuableList().Count);
    }

    [Fact]
    public void ProbeBlocksPattern()
    {
        var answers = GetAnswers("b1(R)?");
        Assert.Single(answers);
        Assert.True(answers[0].IsOk);
    }
}
