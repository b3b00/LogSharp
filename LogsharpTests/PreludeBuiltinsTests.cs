using logSharp.model;
using NFluent;
using Xunit;

namespace ogsharpTests;

public class PreludeBuiltinsTests : BaseTest
{
    public PreludeBuiltinsTests() : base(PrologProgram)
    {
    }
    
    public const string PrologProgram = @"
mapper(a,aa).
mapper(b,bb).
mapper(c,cc).

relation(a,a).
relation(a,aa).
relation(a,aaa).
relation(b,bb).
relation(c,cc).

";

    [Fact]
    public void MapListTest()
    {
        var answers = GetAnswers("map_list([a,b,c],mapper(X,Y),L)?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0]).IsOk();
        var unification = answers[0];
        Check.That(unification.HasBinding("L")).IsTrue();
        var value =  unification.GetBinding("L");
        Check.That(value).IsNotNull();
        Check.That(value.IsList).IsTrue();
        Check.That(value).IsInstanceOf<Term>();
        var term = value as Term;
        var lst = term.ToValuableList();
        Check.That(lst.Count).IsEqualTo(3);
        Check.That(lst.Extracting(x => x.ToString())).IsEquivalentTo("aa","bb","cc");
        Check.That(value).IsNotNull();
    } 
    
    [Fact]
    public void BagOfTest()
    {
        var answers = GetAnswers("bag_of(X,relation(a,X), L)?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0]).IsOk();
        var unification = answers[0];
        Check.That(unification.HasBinding("L")).IsTrue();
        var value =  unification.GetBinding("L");
        Check.That(value).IsNotNull();
        Check.That(value.IsList).IsTrue();
        Check.That(value).IsInstanceOf<Term>();
        var term = value as Term;
        var lst = term.ToValuableList();
        Check.That(lst.Count).IsEqualTo(3);
        Check.That(lst.Extracting(x => x.ToString())).IsEquivalentTo("a","aa","aaa");
        Check.That(value).IsNotNull();
    } 
    
    [Fact]
    public void MemberFirstTest()
    {
        var answers = GetAnswers("member(a,[a,b,c])?");
        Assert.Single(answers);
        Assert.True(answers[0].IsOk);
    }
        
    [Fact]
    public void MemberSecondTest()
    {
        var answers = GetAnswers("member(b,[a,b,c])?");
        Assert.Single(answers);
        Assert.True(answers[0].IsOk);
    }
        
    [Fact]
    public void MemberThirdTest()
    {
        var answers = GetAnswers("member(c,[a,b,c])?");
        Assert.Single(answers);
        Assert.True(answers[0].IsOk);
    }
}