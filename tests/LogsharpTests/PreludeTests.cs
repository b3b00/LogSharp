using System.Collections.Generic;
using System.Linq;
using logSharp.interpretation.unification;
using logSharp.model;
using NFluent;
using Xunit;

namespace LogsharpTests;

// tests for the predicates provided by the embedded prelude library
// (src/LogSharp/prelude/prelude.pro), which is loaded by every Interpreter
public class PreludeTests : BaseTest
{
    // helper rules used by the include/3, exclude/3 and foldl/4 tests
    public const string PrologProgram = @"
big(X) :- number(X) X > 10.
add(X, Y, Z) :- Z is X + Y.
cons(X, Acc, [X | Acc]).
";

    public PreludeTests() : base(PrologProgram)
    {

    }

    private static List<Unification> Ok(List<Unification> answers) =>
        answers.Where(a => a.IsOk).ToList();

    private static List<string> Elements(Unification answer, string variable) =>
        (answer.GetBinding(variable) as Term).ToValuableList().Select(x => x.ToString()).ToList();

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

    // -----------------------------------------------------------------------
    // append/3
    // -----------------------------------------------------------------------

    [Fact]
    public void AppendThreeJoinsTwoLists()
    {
        var answers = Ok(GetAnswers("append([1,2],[3,4],W)?"));
        Check.That(answers).CountIs(1);
        Check.That(Elements(answers[0], "W")).IsEquivalentTo("1", "2", "3", "4");
    }

    // the split of [1,2,3,4] after [3,4] is unique : [1,2] ++ [3,4]
    [Fact]
    public void AppendThreeSplitsWithBoundSuffix()
    {
        var answers = Ok(GetAnswers("append(P,[3,4],[1,2,3,4])?"));
        Check.That(answers).CountIs(1);
        Check.That(Elements(answers[0], "P")).IsEquivalentTo("1", "2");
    }

    [Fact]
    public void AppendThreeFailsWhenWholeDoesNotMatch()
    {
        var answers = Ok(GetAnswers("append([1,5],[3,4],[1,2,3,4])?"));
        Check.That(answers).IsEmpty();
    }

    // -----------------------------------------------------------------------
    // prefix/2 and suffix/2
    // -----------------------------------------------------------------------

    [Fact]
    public void PrefixSucceedsForAnInitialSegment()
    {
        var answers = Ok(GetAnswers("prefix([1],[1,2,3])?"));
        Check.That(answers).CountIs(1);
    }

    [Fact]
    public void PrefixFailsForANonInitialSegment()
    {
        var answers = Ok(GetAnswers("prefix([2],[1,2,3])?"));
        Check.That(answers).IsEmpty();
    }

    // prefix(P, Whole) enumerates every prefix of a bound Whole
    [Fact]
    public void PrefixGeneratesEveryPrefix()
    {
        var answers = Ok(GetAnswers("prefix(P,[1,2,3])?"));
        Check.That(answers).CountIs(4);
        var prefixes = answers.Select(a => Elements(a, "P").Count()).ToList();
        Check.That(prefixes).ContainsExactly(3, 2, 1, 0);
    }

    [Fact]
    public void SuffixSucceedsForAFinalSegment()
    {
        var answers = Ok(GetAnswers("suffix([2,3],[1,2,3])?"));
        Check.That(answers).CountIs(1);
    }

    [Fact]
    public void SuffixGeneratesEverySuffix()
    {
        var answers = Ok(GetAnswers("suffix(S,[1,2,3])?"));
        Check.That(answers).CountIs(4);
        var suffixes = answers.Select(a => Elements(a, "S").Count()).ToList();
        Check.That(suffixes).ContainsExactly(0, 1, 2, 3);
    }

    // -----------------------------------------------------------------------
    // sublist/2
    // -----------------------------------------------------------------------

    [Fact]
    public void SublistSucceedsForAContiguousSlice()
    {
        var answers = Ok(GetAnswers("sublist([2,3],[1,2,3,4])?"));
        Check.That(answers).CountIs(1);
    }

    [Fact]
    public void SublistFailsForANonContiguousSlice()
    {
        var answers = Ok(GetAnswers("sublist([1,3],[1,2,3,4])?"));
        Check.That(answers).IsEmpty();
    }

    // -----------------------------------------------------------------------
    // last/2
    // -----------------------------------------------------------------------

    [Fact]
    public void LastReturnsTheLastElement()
    {
        var answers = Ok(GetAnswers("last([a,b,c],X)?"));
        Check.That(answers).CountIs(1);
        Check.That(answers[0].GetBinding("X").ToString()).IsEqualTo("c");
    }

    [Fact]
    public void LastChecksTheLastElement()
    {
        var answers = Ok(GetAnswers("last([a,b,c],c)?"));
        Check.That(answers).CountIs(1);

        answers = Ok(GetAnswers("last([a,b,c],b)?"));
        Check.That(answers).IsEmpty();
    }

    [Fact]
    public void LastFailsOnAnEmptyList()
    {
        var answers = Ok(GetAnswers("last([],X)?"));
        Check.That(answers).IsEmpty();
    }

    // -----------------------------------------------------------------------
    // nth0/3 and nth1/3
    // -----------------------------------------------------------------------

    [Fact]
    public void Nth0AccessesAnElementByBoundIndex()
    {
        var answers = Ok(GetAnswers("nth0(1,[a,b,c],E)?"));
        Check.That(answers).CountIs(1);
        Check.That(answers[0].GetBinding("E").ToString()).IsEqualTo("b");
    }

    [Fact]
    public void Nth0EnumeratesEveryIndexAndElement()
    {
        var answers = Ok(GetAnswers("nth0(I,[a,b,c],E)?"));
        Check.That(answers).CountIs(3);
        var elements = answers.Select(a => a.GetBinding("E").ToString()).ToList();
        Check.That(elements).ContainsExactly("c", "b", "a");
        var indices = answers.Select(a => a.GetBinding("I").ToString()).ToList();
        Check.That(indices).ContainsExactly("2", "1", "0");
    }

    [Fact]
    public void Nth0FailsWhenIndexIsOutOfRange()
    {
        var answers = Ok(GetAnswers("nth0(3,[a,b,c],E)?"));
        Check.That(answers).IsEmpty();
    }

    [Fact]
    public void Nth1NumbersElementsFromOne()
    {
        var answers = Ok(GetAnswers("nth1(2,[a,b,c],E)?"));
        Check.That(answers).CountIs(1);
        Check.That(answers[0].GetBinding("E").ToString()).IsEqualTo("b");
    }

    // -----------------------------------------------------------------------
    // reverse/2
    // -----------------------------------------------------------------------

    [Fact]
    public void ReverseReversesTheList()
    {
        var answers = Ok(GetAnswers("reverse([1,2,3],R)?"));
        Check.That(answers).CountIs(1);
        Check.That(Elements(answers[0], "R")).IsEquivalentTo("3", "2", "1");
    }

    [Fact]
    public void ReverseChecksAnAlreadyReversedList()
    {
        var answers = Ok(GetAnswers("reverse([1,2,3],[3,2,1])?"));
        Check.That(answers).CountIs(1);

        answers = Ok(GetAnswers("reverse([1,2,3],[1,2,3])?"));
        Check.That(answers).IsEmpty();
    }

    // -----------------------------------------------------------------------
    // sum_list/2, max_list/2 and min_list/2
    // -----------------------------------------------------------------------

    [Fact]
    public void SumListSumsTheElements()
    {
        var answers = Ok(GetAnswers("sum_list([1,2,3],S)?"));
        Check.That(answers).CountIs(1);
        Check.That(answers[0].GetBinding("S").ToString()).IsEqualTo("6");
    }

    [Fact]
    public void SumListOnAnEmptyListIsZero()
    {
        var answers = Ok(GetAnswers("sum_list([],S)?"));
        Check.That(answers).CountIs(1);
        Check.That(answers[0].GetBinding("S").ToString()).IsEqualTo("0");
    }

    [Fact]
    public void MaxListReturnsTheGreatestElement()
    {
        var answers = Ok(GetAnswers("max_list([1,5,3],M)?"));
        Check.That(answers).CountIs(1);
        Check.That(answers[0].GetBinding("M").ToString()).IsEqualTo("5");
    }

    [Fact]
    public void MaxListOnASingleElementListIsThatElement()
    {
        var answers = Ok(GetAnswers("max_list([7],M)?"));
        Check.That(answers).CountIs(1);
        Check.That(answers[0].GetBinding("M").ToString()).IsEqualTo("7");
    }

    [Fact]
    public void MinListReturnsTheSmallestElement()
    {
        var answers = Ok(GetAnswers("min_list([1,5,3],M)?"));
        Check.That(answers).CountIs(1);
        Check.That(answers[0].GetBinding("M").ToString()).IsEqualTo("1");
    }

    // -----------------------------------------------------------------------
    // memberchk/2
    // -----------------------------------------------------------------------

    [Fact]
    public void MemberchkSucceedsExactlyOnceWhenTheElementOccurs()
    {
        var answers = Ok(GetAnswers("memberchk(b,[a,b,c])?"));
        Check.That(answers).CountIs(1);
    }

    [Fact]
    public void MemberchkFailsWhenTheElementIsMissing()
    {
        var answers = Ok(GetAnswers("memberchk(z,[a,b,c])?"));
        Check.That(answers).IsEmpty();
    }

    // -----------------------------------------------------------------------
    // between/3
    // -----------------------------------------------------------------------

    [Fact]
    public void BetweenEnumeratesEveryValueInTheRange()
    {
        var answers = Ok(GetAnswers("between(1,3,X)?"));
        Check.That(answers).CountIs(3);
        var values = answers.Select(a => a.GetBinding("X").ToString()).ToList();
        Check.That(values).ContainsExactly("3", "2", "1");
    }

    [Fact]
    public void BetweenChecksThatAValueIsInTheRange()
    {
        var answers = Ok(GetAnswers("between(1,3,2)?"));
        Check.That(answers).CountIs(1);

        // no duplicate solution : the value 2 is only enumerated once
        answers = Ok(GetAnswers("between(1,3,5)?"));
        Check.That(answers).IsEmpty();
    }

    // -----------------------------------------------------------------------
    // range/3
    // -----------------------------------------------------------------------

    [Fact]
    public void RangeBuildsTheInclusiveListOfIntegers()
    {
        var answers = Ok(GetAnswers("range(1,4,L)?"));
        Check.That(answers).CountIs(1);
        Check.That(Elements(answers[0], "L")).IsEquivalentTo("1", "2", "3", "4");
    }

    [Fact]
    public void RangeWithLowGreaterThanHighIsEmpty()
    {
        var answers = Ok(GetAnswers("range(3,1,L)?"));
        Check.That(answers).CountIs(1);
        Check.That(Elements(answers[0], "L")).IsEmpty();
    }

    // -----------------------------------------------------------------------
    // select/3
    // -----------------------------------------------------------------------

    [Fact]
    public void SelectRemovesOneOccurrence()
    {
        var answers = Ok(GetAnswers("select(b,[a,b,c],R)?"));
        Check.That(answers).CountIs(1);
        Check.That(Elements(answers[0], "R")).IsEquivalentTo("a", "c");
    }

    [Fact]
    public void SelectWithAFreeElementPicksEachElementInTurn()
    {
        var answers = Ok(GetAnswers("select(X,[a,b],R)?"));
        Check.That(answers).CountIs(2);
        var elements = answers.Select(a => a.GetBinding("X").ToString()).ToList();
        Check.That(elements).ContainsExactly("b", "a");
    }

    [Fact]
    public void SelectFailsWhenTheElementIsMissing()
    {
        var answers = Ok(GetAnswers("select(z,[a,b,c],R)?"));
        Check.That(answers).IsEmpty();
    }

    // -----------------------------------------------------------------------
    // permutation/2
    // -----------------------------------------------------------------------

    [Fact]
    public void PermutationEnumeratesEveryPermutation()
    {
        var answers = Ok(GetAnswers("permutation([a,b],P)?"));
        Check.That(answers).CountIs(2);
        var permutations = answers.Select(a => Elements(a, "P")).ToList();
        Check.That(permutations[0]).IsEquivalentTo("b", "a");
        Check.That(permutations[1]).IsEquivalentTo("a", "b");
    }

    [Fact]
    public void PermutationChecksAGivenPermutation()
    {
        var answers = Ok(GetAnswers("permutation([a,b],[b,a])?"));
        Check.That(answers).CountIs(1);

        answers = Ok(GetAnswers("permutation([a,b],[b,b])?"));
        Check.That(answers).IsEmpty();
    }

    // -----------------------------------------------------------------------
    // include/3 and exclude/3
    // -----------------------------------------------------------------------

    [Fact]
    public void IncludeKeepsTheElementsSatisfyingTheGoal()
    {
        var answers = Ok(GetAnswers("include(big,[5,11,12,3],I)?"));
        Check.That(answers).CountIs(1);
        Check.That(Elements(answers[0], "I")).IsEquivalentTo("11", "12");
    }

    [Fact]
    public void IncludeOnAnEmptyListIsEmpty()
    {
        var answers = Ok(GetAnswers("include(big,[],I)?"));
        Check.That(answers).CountIs(1);
        Check.That(Elements(answers[0], "I")).IsEmpty();
    }

    [Fact]
    public void ExcludeDropsTheElementsSatisfyingTheGoal()
    {
        var answers = Ok(GetAnswers("exclude(big,[5,11,12,3],E)?"));
        Check.That(answers).CountIs(1);
        Check.That(Elements(answers[0], "E")).IsEquivalentTo("5", "3");
    }

    // -----------------------------------------------------------------------
    // foldl/4
    // -----------------------------------------------------------------------

    [Fact]
    public void FoldlReducesAListFromLeftToRight()
    {
        var answers = Ok(GetAnswers("foldl(add,[1,2,3],0,S)?"));
        Check.That(answers).CountIs(1);
        Check.That(answers[0].GetBinding("S").ToString()).IsEqualTo("6");
    }

    [Fact]
    public void FoldlOnAnEmptyListReturnsTheAccumulator()
    {
        var answers = Ok(GetAnswers("foldl(add,[],5,S)?"));
        Check.That(answers).CountIs(1);
        Check.That(answers[0].GetBinding("S").ToString()).IsEqualTo("5");
    }

    [Fact]
    public void FoldlChecksAGivenResult()
    {
        var answers = Ok(GetAnswers("foldl(add,[1,2],0,3)?"));
        Check.That(answers).CountIs(1);

        answers = Ok(GetAnswers("foldl(add,[1,2],0,4)?"));
        Check.That(answers).IsEmpty();
    }

    // cons(Head, Acc, [Head | Acc]) prepends each element to the accumulator,
    // so a left fold collects the elements in reverse order
    [Fact]
    public void FoldlProcessesTheElementsInListOrder()
    {
        var answers = Ok(GetAnswers("foldl(cons,[1,2,3],[],R)?"));
        Check.That(answers).CountIs(1);
        Check.That(Elements(answers[0], "R")).IsEquivalentTo("3", "2", "1");
    }
}
