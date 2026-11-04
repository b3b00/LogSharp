using System.Collections.Generic;
using System.Linq;
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

    [Fact]
    public void LengthFromBoundListToFreeVariableTest()
    {
        var answers = GetAnswers("length([a,b,c],L)?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0].IsOk).IsTrue();
        Check.That(answers[0].GetBinding("L")).IsEqualTo(new Int(3));
    }

    [Fact]
    public void LengthGeneratedListTest()
    {
        // two ways binding : length is bound, list is free
        var answers = GetAnswers("length(L,3)?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0].IsOk).IsTrue();
        var boundList = answers[0].GetBinding("L") as Term;
        Check.That(boundList).IsNotNull();
        Check.That(boundList.IsList).IsTrue();
        Check.That(boundList.ToValuableList().Count).IsEqualTo(3);
    }

    [Fact]
    public void LengthGeneratedEmptyListTest()
    {
        var answers = GetAnswers("length(L,0)?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0].IsOk).IsTrue();
        Check.That(answers[0].GetBinding("L")).IsEqualTo(Term.ListTerm(new List<Valuable>()));
    }

    [Fact]
    public void SortToVariableTest()
    {
        var answers = GetAnswers("sort([c,a,b],S)?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0].IsOk).IsTrue();
        var sorted = answers[0].GetBinding("S") as Term;
        Check.That(sorted).IsNotNull();
        Check.That(string.Join(",", sorted.ToValuableList().Select(i => i.Dump()))).IsEqualTo("a,b,c");
    }

    [Fact]
    public void SortCheckBoundResultTest()
    {
        var answers = GetAnswers("sort([c,a,b],[a,b,c])?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0].IsOk).IsTrue();
    }

    [Fact]
    public void SortWrongBoundResultTest()
    {
        var answers = GetAnswers("sort([c,a,b],[c,b,a])?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0].IsOk).IsFalse();
    }

    [Fact]
    public void SortRemovesDuplicatesTest()
    {
        var answers = GetAnswers("sort([b,a,b,c,a],S)?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0].IsOk).IsTrue();
        var sorted = answers[0].GetBinding("S") as Term;
        Check.That(sorted).IsNotNull();
        Check.That(string.Join(",", sorted.ToValuableList().Select(i => i.Dump()))).IsEqualTo("a,a,b,b,c");
    }

    [Fact]
    public void SortNumbersTest()
    {
        var answers = GetAnswers("sort([3,1,2],S)?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0].IsOk).IsTrue();
        var sorted = answers[0].GetBinding("S") as Term;
        Check.That(sorted).IsNotNull();
        Check.That(string.Join(",", sorted.ToValuableList().Select(i => i.Dump()))).IsEqualTo("1,2,3");
    }

    [Fact]
    public void SortTwoWaysBindingTest()
    {
        // two ways binding : sorted result is bound, list is free
        var answers = GetAnswers("sort(L,[a,b,c])?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0].IsOk).IsTrue();
        var boundList = answers[0].GetBinding("L") as Term;
        Check.That(boundList).IsNotNull();
        Check.That(string.Join(",", boundList.ToValuableList().Select(i => i.Dump()))).IsEqualTo("a,b,c");
    }
    
    [Fact]
    public void TrickierSortTwoWaysBindingTest()
    {
        // two ways binding : sorted result is bound, list is free
        var answers = GetAnswers("sort([a,b,c],[A,B,C])?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0]).IsOk();
        Check.That(answers[0]).Contains("A","a");
        Check.That(answers[0]).Contains("B","b");
        Check.That(answers[0]).Contains("C","c");
    }
    
    [Fact]
    public void TrickierSortTwoWaysBindingWithDiscardBindingsTest()
    {
        // two ways binding : sorted result is bound, list is free
        var answers = GetAnswers("sort([a,b,c,d,e,f],[_, X, T])?");
        Check.That(answers).CountIs(1);
        var answer = answers[0];
        Check.That(answer.IsOk).IsTrue();
        Check.That(answer).Contains("X","b");
        Check.That(answer).Contains("T", "[c, d, e, f]");
    }
    
    [Fact]
    public void TrickierSortTwoWaysBindingWithDiscardBindingsTest2()
    {
        // two ways binding : sorted result is bound, list is free
        var answers = GetAnswers("sort([a,b,c,d,e,f],[_, X, Y, T])?");
        Check.That(answers).CountIs(1);
        var answer = answers[0];
        Check.That(answer.IsOk).IsTrue();
        Check.That(answer).Contains("X","b");
        Check.That(answer).Contains("Y","c");
        Check.That(answer).Contains("T", "[d, e, f]");
    }
    
    [Fact]
    public void TrickierSortTwoWaysBindingWithDiscardBindingsTest3()
    {
        // two ways binding : sorted result is bound, list is free
        var answers = GetAnswers("sort([a,b,c,d,e,f],[_, X, _, Y, T])?");
        Check.That(answers).CountIs(1);
        var answer = answers[0];
        Check.That(answer.IsOk).IsTrue();
        Check.That(answer).Contains("X","b");
        Check.That(answer).Contains("Y","d");
        Check.That(answer).Contains("T", "[e, f]");
    }
    
    
    
    
    
    
}

