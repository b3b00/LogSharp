using System.Collections.Generic;
using System.Linq;
using logSharp.model;
using NFluent;
using Xunit;

namespace LogsharpTests;

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

callable_0().

five(a,b,c,d,e).
five(f,g,h,j,k).
five(l,m,n,o,p).

callable_1(X) :- single(X).
mapped_and_found() :- map_list([a,b,c],mapper(X,Y),L) member(bb,L).
mapped_no_aa() :- map_list([a,b,c],mapper(X,Y),L) not(member(aa,L)).
rules_then_length(L,N) :- map_list([a,b],mapper(X,Y),L) length(L,N).
length_one_shot(X) :- length([a,b,c],N) single(X).
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
    public void FindAllTest()
    {
        var answers = GetAnswers("find_all(X,relation(a,X), L)?");
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
        var answers = GetAnswers("call(single(X))?");
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
        var answers = GetAnswers("not(five,a,b,c,d,e)?");
        Check.That(answers[0]).IsNotNull();
        Check.That(answers[0]).Not.IsOk();
        answers = GetAnswers("not(five,whatever,nothing,nope,_,U)?");
        Check.That(answers[0]).IsNotNull();
        Check.That(answers[0]).Not.IsOk();
        
    }

    [Fact]
    public void Not_1test()
    {
        var answers = GetAnswers("call(single(a))?");
        Check.That(answers[0]).IsNotNull();
        Check.That(answers[0]).Not.IsOk();
        answers = GetAnswers("call(single(whatever))?");
        Check.That(answers[0]).IsNotNull();
        Check.That(answers[0]).Not.IsOk();
    }
    
    [Fact]
    public void Not_6test()
    {
        var answers = GetAnswers("call(five,X,Y,Z,W, K)?");
        Check.That(answers[0]).IsNotNull();
        Check.That(answers[0]).Not.IsOk();
        answers = GetAnswers("call(single(whatever))?");
        Check.That(answers[0]).IsNotNull();
        Check.That(answers[0]).Not.IsOk();
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
        // the pattern [_ , X | T] discards the head, binds X to the second
        // element and T to the remaining tail
        var answers = GetAnswers("sort([a,b,c,d,e,f],[_, X | T])?");
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
        var answers = GetAnswers("sort([a,b,c,d,e,f],[_, X, Y | T])?");
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
        var answers = GetAnswers("sort([a,b,c,d,e,f],[_, X, _, Y | T])?");
        Check.That(answers).CountIs(1);
        var answer = answers[0];
        Check.That(answer.IsOk).IsTrue();
        Check.That(answer).Contains("X","b");
        Check.That(answer).Contains("Y","d");
        Check.That(answer).Contains("T", "[e, f]");
    }

    //-----------------------------------------------------------
    // Regression tests for the continuation re-link bug
    // (see childofbug.md) : bindings produced by a built-in used as a
    // NON-final conjunct must survive into the next conjuncts, and the
    // remaining body goals must NOT be executed twice.

    [Fact]
    public void MapListAsFirstConjunctTest()
    {
        // member/2 consumes the body-local list L produced by map_list/3
        var answers = GetAnswers("mapped_and_found()?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0]).IsOk();
    }

    [Fact]
    public void MapListWithNotTest()
    {
        // not/1 consumes the body-local list L produced by map_list/3.
        // "aa" IS in the mapped list, so not(member(aa,L)) must fail the rule.
        // If L is lost before the second conjunct, member fails and not succeeds
        // (false positive) : this test discriminates both behaviors.
        var answers = GetAnswers("mapped_no_aa()?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0]).Not.IsOk();
    }

    [Fact]
    public void LengthAfterMapListTest()
    {
        // length/2 consumes the body-local list L produced by map_list/3,
        // and binds N into the rule head.
        var answers = GetAnswers("rules_then_length(L,N)?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0]).IsOk();
        Check.That(answers[0]).Contains("N", "2");
        var boundList = answers[0].GetBinding("L") as Term;
        Check.That(boundList).IsNotNull();
        Check.That(boundList.IsList).IsTrue();
        Check.That(boundList.ToValuableList().Count).IsEqualTo(2);
    }

    [Fact]
    public void BuiltInNotReExecutedAfterItsContinuationTest()
    {
        // length/2 binds N, then single/1 produces 2 solutions.
        // The re-link bug re-executed length (and all the remaining goals)
        // for each completed continuation, duplicating every answer.
        var answers = GetAnswers("length_one_shot(X)?");
        Check.That(answers).CountIs(2);
        Check.That(answers.Extracting(x => x.GetBinding("X").ToString()))
            .IsEquivalentTo(new List<string>() { "a", "b" });
    }

    [Fact]
    public void PreludeDistinctElementsFastTest()
    {
        // prelude rule: sort(List, Sorted) length(List, Len) length(Sorted, Len)
        // "Sorted" is a body-local variable produced by sort/2 and consumed by
        // the last length/2 conjunct : exactly the shape that used to be
        // broken be the continuation re-link bug.
        var answers = GetAnswers("distinct_elements_fast([c,b,a])?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0]).IsOk();

        answers = GetAnswers("distinct_elements_fast([c,b,c])?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0]).Not.IsOk();
    }

    
    
    
    
    
}