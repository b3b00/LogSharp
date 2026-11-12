using System;
using System.Collections.Generic;
using System.Linq;
using logSharp.interpretation.unification;
using logSharp.model;
using NFluent;
using Xunit;

namespace LogsharpTests;

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
        Check.That(scoresAsStrings.Count).IsEqualTo(6);
        Check.That(scoresAsStrings).IsEquivalentTo(new List<string>() { "85", "90", "75", "80", "10","10" });
    }


    // covers the bound output list branch of find_all/3
    // (find_all(X, Query, [already bound list]) -> success when the list
    // contains exactly the values collected from Query)
    [Fact]
    public void TestFindAllWithBoundOutputListSucceedsWhenListMatches()
    {
        var answers = GetAnswers("find_all(Score, grade(alice, _, Score), [85, 90])?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0].IsOk).IsTrue();
    }

    [Fact]
    public void TestFindAllWithBoundOutputListIsOrderInsensitive()
    {
        var answers = GetAnswers("find_all(Score, grade(alice, _, Score), [90, 85])?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0].IsOk).IsTrue();
    }

    [Fact]
    public void TestFindAllWithBoundOutputListFailsWhenValueDiffers()
    {
        var answers = GetAnswers("find_all(Score, grade(alice, _, Score), [85, 91])?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0].IsOk).IsFalse();
    }

    [Fact]
    public void TestFindAllWithBoundOutputListFailsWhenLengthDiffers()
    {
        var answers = GetAnswers("find_all(Score, grade(alice, _, Score), [85, 90, 75])?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0].IsOk).IsFalse();
    }

    [Fact]
    public void TestFindAllWithBoundOutputListFailsWhenQueryHasNoSolution()
    {
        var answers = GetAnswers("find_all(Score, grade(charlie, _, Score), [85, 90])?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0].IsOk).IsFalse();
    }

    [Fact]
    public void TestFindAllFailsWhenThirdParameterIsNeitherVariableNorList()
    {
        var answers = GetAnswers("find_all(Score, grade(alice, math, Score), 85)?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0].IsOk).IsFalse();
    }

    [Fact]
    public void TestFindAllFailsWhenFirstParameterIsNotAVariable()
    {
        var answers = GetAnswers("find_all(85, grade(alice, math, Score), [85, 90])?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0].IsOk).IsFalse();
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

        CheckItem("bob", ["10","10", "80", "75"], answers);
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

        CheckItem("bob", "science", ["10", "10", "80"], answers);
        CheckItem("bob", "math", ["75"], answers);
        CheckItem("alice", "science", ["90"], answers);
        CheckItem("alice", "math", ["85"], answers);
    }
    
    [Fact]
    public void TestSetOfWithMultipleFreeVariables()
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
        
        
        var answers = GetAnswers("set_of(Score, grade(Student, Subject, Score), Scores)?");
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

    // covers the template resolution of bag_of/3 : template is a Term,
    // evaluated against each result environment (pair(Subject, Score))
    [Fact]
    public void TestBagOfWithTermTemplate()
    {
        var answers = GetAnswers("bag_of(pair(Subject, Score), grade(Student, Subject, Score), L)?");
        Check.That(answers).CountIs(2);

        var alice = answers.FirstOrDefault(a => a.GetBinding("Student")?.ToString() == "alice");
        Check.That(alice).IsNotNull();
        Check.That(alice.IsOk).IsTrue();
        Check.That(alice).Contains("L");
        var alicePairs = (alice.GetBinding("L") as Term).ToValuableList();
        Check.That(alicePairs.Count).IsEqualTo(2);
        Check.That(alicePairs.Extracting(x => x.ToString()))
            .IsEquivalentTo("pair(math, 85)", "pair(science, 90)");

        var bob = answers.FirstOrDefault(a => a.GetBinding("Student")?.ToString() == "bob");
        Check.That(bob).IsNotNull();
        Check.That(bob.IsOk).IsTrue();
        var bobPairs = (bob.GetBinding("L") as Term).ToValuableList();
        Check.That(bobPairs.Count).IsEqualTo(4);
        Check.That(bobPairs.Extracting(x => x.ToString()))
            .IsEquivalentTo("pair(math, 75)", "pair(science, 80)", "pair(science, 10)", "pair(science, 10)");
    }

    // covers the template resolution of bag_of/3 : template is a Scalar,
    // reused as-is for every matching result (bag is grouped per Score)
    [Fact]
    public void TestBagOfWithScalarTemplate()
    {
        var answers = GetAnswers("bag_of(85, grade(alice, _, Score), L)?");
        Check.That(answers).CountIs(2);
        foreach (var answer in answers)
        {
            Check.That(answer.IsOk).IsTrue();
            Check.That(answer).Contains("L");
            var values = (answer.GetBinding("L") as Term).ToValuableList();
            Check.That(values.Count).IsEqualTo(1);
            Check.That(values[0].ToString()).IsEqualTo("85");
        }
    }

    // covers the template resolution of bag_of/3 : template is a Variable
    // that the query never binds -> the raw variable is collected
    [Fact]
    public void TestBagOfWithNeverBoundVariableTemplate()
    {
        var answers = GetAnswers("bag_of(Z, grade(alice, math, 85), L)?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0].IsOk).IsTrue();
        Check.That(answers[0]).Contains("L");
        var values = (answers[0].GetBinding("L") as Term).ToValuableList();
        Check.That(values.Count).IsEqualTo(1);
        Check.That(values[0]).IsInstanceOf<Variable>();
        Check.That((values[0] as Variable).Name).IsEqualTo("Z");
    }
}
