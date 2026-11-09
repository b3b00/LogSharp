using System.Collections.Generic;
using System.Linq;
using logSharp.model;
using NFluent;
using Xunit;

namespace ogsharpTests;

public class AllSolutionsPredicateTests : BaseTest
{
    public AllSolutionsPredicateTests() : base(PrologProgram)
    {
    }
        
    public const string PrologProgram = @"
grade(alice, math, 85).
grade(alice, science, 90).
grade(bob, math, 75).
grade(bob, science, 80).
";

    [Fact]
    public void TestFindAll()
    {
        var answers = GetAnswers("find_all(Score, grade(Student, Subject, Score), Scores)?");
        Check.That(answers).CountIs(1);
        var answer = answers[0];
        Check.That(answer).IsOk();
        Check.That(answer).Contains("Scores");
        var scores = answer.GetBinding("Scores");
        Check.That(scores.IsTerm).IsTrue();
        Check.That(scores.IsList).IsTrue();
        var scoresAsStrings = (scores as Term).ToValuableList().Select(x => x.ToString()).ToList();
        Check.That(scoresAsStrings).IsNotNull();
        Check.That(scoresAsStrings.Count).IsEqualTo(4);
        Check.That(scoresAsStrings).IsEquivalentTo(new List<string>() { "85", "90", "75", "80" });
    }


    public void TestBagOf()
    {
        var answers = GetAnswers("bag_of(Score, grade(Student, Subject, Score), Scores)?");
        Check.That(answers).CountIs(2);
        /*
        Solution 1: Student = alice, Scores = [85, 90]
        Solution 2: Student = bob, Scores = [75, 80]
        */
    }
}