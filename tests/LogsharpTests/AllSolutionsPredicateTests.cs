using System;
using System.Collections.Generic;
using System.Linq;
using logSharp.interpretation.unification;
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
grade(bob, science, 10).
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
        Check.That(scoresAsStrings.Count).IsEqualTo(5);
        Check.That(scoresAsStrings).IsEquivalentTo(new List<string>() { "85", "90", "75", "80", "10" });
    }


    [Fact]
    public void TestBagOf()
    {
        Action<string,  string[], List<Unification>> CheckItem =
            (student, expectedScores, answers) =>
            {
                var answer = answers.FirstOrDefault(a => a.GetBinding("Student")?.ToString() == student );
                Check.That(answer).IsNotNull();
                Check.That(answer.IsOk).IsTrue();
                Check.That(answer).Contains("Scores");
                var scores1 = (answer.GetBinding("Scores") as Term).ToValuableList().Select(x => x.ToString()).ToList();
                Check.That(scores1).IsEquivalentTo(expectedScores);
            };
        
        
        var answers = GetAnswers("bag_of(Score, grade(Student, _, Score), Scores)?");
        Check.That(answers).CountIs(2);

        CheckItem("bob", ["10", "80", "75"], answers);
        CheckItem("alice", ["90","85"], answers);
    }

    [Fact]
    public void TestBagOfWithMultipleFreeVariables()
    {
        Action<string, string, string[], List<Unification>> CheckItem =
            (student, subject, expectedScores, answers) =>
            {
                var answer = answers.FirstOrDefault(a => a.GetBinding("Student")?.ToString() == student && a.GetBinding("Subject")?.ToString() == subject);
                Check.That(answer).IsNotNull();
                Check.That(answer.IsOk).IsTrue();
                Check.That(answer).Contains("Scores");
                var scores1 = (answer.GetBinding("Scores") as Term).ToValuableList().Select(x => x.ToString()).ToList();
                Check.That(scores1).IsEquivalentTo(expectedScores);
            };
        
        
        var answers = GetAnswers("bag_of(Score, grade(Student, Subject, Score), Scores)?");
        Check.That(answers).CountIs(4);

        CheckItem("bob", "science", ["10", "80"], answers);
        CheckItem("bob", "math", ["75"], answers);
        CheckItem("alice", "science", ["90"], answers);
        CheckItem("alice", "math", ["85"], answers);
    }

    [Fact]
    public void TestBagOfFailWhenNoSolutions()
    {
        var answers = GetAnswers("bag_of(Score, grade(charlie, _, Score), Scores)?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0].IsOk).IsFalse();
    }

    [Fact]
    public void TestBagOfBoundOutputList()
    {
        var answers = GetAnswers("bag_of(Score, grade(Student, _, Score), [90, 85])?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0].IsOk).IsTrue();
        Check.That(answers[0].GetBinding("Student").ToString()).IsEqualTo("alice");
    }
}
