using System.Collections.Generic;
using logSharp.model;
using NFluent;
using Xunit;

namespace ogsharpTests;

public class PrologBuiltinsTests : BaseTest
{
    public PrologBuiltinsTests() : base(PrologProgram)
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

single(a).
single(b).

callable_0.

five(a,b,c,d,e).
five(f,g,h,j,k).
five(l,m,n,o,p).

callable_1(X) :- single(X).

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
    public void Call_0Test()
    {
        var answers = GetAnswers("call(callable_0)?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0]).IsNotNull();
        var unification = answers[0];
        Check.That(unification).IsOk();
    }
    
    [Fact]
    public void Call_1Test()
    {
        var answers = GetAnswers("call(single,X)?");
        Check.That(answers[0]).IsNotNull();
        Check.That(answers).CountIs(2);
        Check.That(answers.Extracting(x => x.GetBinding("X").ToString()))
            .IsEquivalentTo(new List<string>() { "a", "b" });
    }
    
    [Fact]
    public void Call_2Test()
    {
        var answers = GetAnswers("call(relation,X,Y)?");
        Check.That(answers[0]).IsNotNull();
        Check.That(answers).CountIs(5);
        Check.That(answers.Extracting(x => x.GetBinding("X")+"-"+x.GetBinding("Y")))
            .IsEquivalentTo(new List<string>() { "a-a", "a-aa","a-aaa","b-bb","c-cc" });
    }
    
    [Fact]
    public void Call_6Test()
    {
        var answers = GetAnswers("call(five,X,Y,Z,W, K)?");
        Check.That(answers[0]).IsNotNull();
        Check.That(answers).CountIs(3);
        Check.That(answers.Extracting(x => x.GetBinding("X")+"-"+x.GetBinding("Y")+"-"+x.GetBinding("Z")+"-"+x.GetBinding("W")+"-"+x.GetBinding("K")))
            .IsEquivalentTo(new List<string>() { "a-b-c-d-e","f-g-h-j-k","l-m-n-o-p"});
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
    
    [Fact]
    public void LengthToVariableTest()
    {
        var answers = GetAnswers("length([a,b,c],L)?");
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
    public void CheckLengthTest()
    {
        var answers = GetAnswers("length([a,b,c],3)?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0]).IsNotNull();
        Check.That(answers[0]).IsOk();
    }
    
    
}