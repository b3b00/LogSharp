using System.Collections.Generic;
using System.Linq;
using logSharp.interpretation;
using logSharp.interpretation.unification;
using logSharp.model;
using NFluent;
using Xunit;

namespace LogsharpTests;

public class DynamicUpdatesTests : BaseTest
{
    public DynamicUpdatesTests() : base(DynamicProgram)
    {
    }

    public const string DynamicProgram = @"
% dynamic database fixtures
likes(bob, pasta).
likes(bob, pizza).
likes(mary, sushi).

counter(1).
counter(2).

item(1).
item(2).
item(3).

% choice points over item/1 stay active while retract wipes the predicate
wipe() :- item(_) retract(item(_)) fail().
";

    private Interpreter BuildInterpreter(string program)
    {
        var parsed = Interpreter.Parser.Parse(program);
        Assert.True(parsed.IsOk);
        return new Interpreter(false, parsed.Result as LogSharpProgram, Interpreter.Parser,
            new NestedTermUnifier());
    }

    private static List<Unification> GetAnswers(Interpreter interpreter, string questionString)
    {
        var parse = interpreter.Parser.Parse(questionString, "question");
        Assert.True(parse.IsOk);
        return interpreter.Search(parse.Result as Question).ToList();
    }

    // -----------------------------------------------------------------------
    // assertz

    [Fact]
    public void AssertzAddsAFactAtTheEnd()
    {
        Check.That(GetAnswers("likes(bob, curry)?")[0].IsOk).IsFalse();

        var assertions = GetAnswers("assertz(likes(bob, curry))?");
        Check.That(assertions).CountIs(1);
        Check.That(assertions[0].IsOk).IsTrue();

        // the asserted clause is APPENDED to the store, in every structure
        var index = Interpreter.LogSharpProgram.GetPredicateIndex("likes/2");
        Check.That(index.AllClauses.Last().ToString()).IsEqualTo("likes(bob, curry).");
        Check.That(Interpreter.LogSharpProgram.RulesBySpec["likes/2"].Last().ToString())
            .IsEqualTo("likes(bob, curry).");

        // and it is served like any other clause of the predicate
        var answers = GetAnswers("likes(bob, What)?");
        Check.That(answers.Extracting(a => a.GetBinding("What").ToString()))
            .IsEquivalentTo(new List<string> { "pasta", "pizza", "curry" });
    }

    [Fact]
    public void AssertzMatchesTheStaticOrdering()
    {
        // reference program : the third clause is written in the source
        var staticInterpreter = BuildInterpreter(@"
likes(bob, pasta).
likes(bob, pizza).
likes(bob, curry).
");

        // dynamic program : the third clause is asserted at runtime
        GetAnswers("assertz(likes(bob, curry))?");
        var dynamicAnswers = GetAnswers("likes(bob, What)?");
        var staticAnswers = GetAnswers(staticInterpreter, "likes(bob, What)?");

        // both programs enumerate the clause in the same position : the
        // assertion keeps the logical ordering of the static clauses
        Check.That(string.Join("|", dynamicAnswers.Select(a => a.GetBinding("What").ToString())))
            .IsEqualTo(string.Join("|", staticAnswers.Select(a => a.GetBinding("What").ToString())));
    }

    [Fact]
    public void AssertzOnUnknownPredicateCreatesIt()
    {
        Check.That(Interpreter.HasRule("new_pred/1")).IsFalse();

        var assertions = GetAnswers("assertz(new_pred(a))?");
        Check.That(assertions[0].IsOk).IsTrue();

        Check.That(Interpreter.HasRule("new_pred/1")).IsTrue();
        Check.That(GetAnswers("new_pred(a)?")[0].IsOk).IsTrue();
        Check.That(GetAnswers("new_pred(b)?")[0].IsOk).IsFalse();
    }

    [Fact]
    public void AssertzClausesAreIndexedOnTheirFirstArgument()
    {
        GetAnswers("assertz(likes(bob, curry))?");

        // the index classified the asserted clause in the bucket of its FIRST
        // argument (bob) and appended it to the fallback list
        var index = Interpreter.LogSharpProgram.GetPredicateIndex("likes/2");
        Check.That(index).IsNotNull();
        Check.That(index.ConstantTable.ContainsKey("bob")).IsTrue();
        Check.That(index.ConstantTable["bob"].Last().ToString()).IsEqualTo("likes(bob, curry).");
        Check.That(index.AllClauses.Count).IsEqualTo(4);

        // the clause is reachable through the index, not only the fallback list
        Check.That(GetAnswers("likes(bob, curry)?")[0].IsOk).IsTrue();
        Check.That(GetAnswers("likes(curry, bob)?")[0].IsOk).IsFalse();
    }

    // -----------------------------------------------------------------------
    // asserta

    [Fact]
    public void AssertaAddsAFactAtTheBeginning()
    {
        var assertions = GetAnswers("asserta(likes(bob, lasagna))?");
        Check.That(assertions[0].IsOk).IsTrue();

        // the asserted clause is PREPENDED to the store, in every structure
        var index = Interpreter.LogSharpProgram.GetPredicateIndex("likes/2");
        Check.That(index.AllClauses.First().ToString()).IsEqualTo("likes(bob, lasagna).");
        Check.That(index.ConstantTable["bob"].First().ToString()).IsEqualTo("likes(bob, lasagna).");

        // and it is served like any other clause of the predicate
        var answers = GetAnswers("likes(bob, What)?");
        Check.That(answers.Extracting(a => a.GetBinding("What").ToString()))
            .IsEquivalentTo(new List<string> { "lasagna", "pasta", "pizza" });
    }

    [Fact]
    public void AssertaMatchesTheStaticOrdering()
    {
        // reference program : the first clause is written in the source
        var staticInterpreter = BuildInterpreter(@"
likes(bob, lasagna).
likes(bob, pasta).
likes(bob, pizza).
");

        // dynamic program : the first clause is asserted at runtime
        GetAnswers("asserta(likes(bob, lasagna))?");
        var dynamicAnswers = GetAnswers("likes(bob, What)?");
        var staticAnswers = GetAnswers(staticInterpreter, "likes(bob, What)?");

        // clauses added via asserta keep the position of a static clause
        // written in front of the predicate
        Check.That(string.Join("|", dynamicAnswers.Select(a => a.GetBinding("What").ToString())))
            .IsEqualTo(string.Join("|", staticAnswers.Select(a => a.GetBinding("What").ToString())));
    }

    [Fact]
    public void AssertaPrependsTheIndexBuckets()
    {
        GetAnswers("asserta(counter(0))?");

        var index = Interpreter.LogSharpProgram.GetPredicateIndex("counter/1");
        Check.That(index.ConstantTable[0].First().ToString()).IsEqualTo("counter(0).");
        Check.That(index.AllClauses.First().ToString()).IsEqualTo("counter(0).");

        // the prepended clause is served, together with the older ones
        var answers = GetAnswers("counter(X)?");
        Check.That(answers.Extracting(a => a.GetBinding("X").ToString()))
            .IsEquivalentTo(new List<string> { "0", "1", "2" });
    }

    // -----------------------------------------------------------------------
    // assert/1

    [Fact]
    public void AssertBehavesLikeAssertzForClauses()
    {
        var assertions = GetAnswers("assert(likes(bob, curry))?");
        Check.That(assertions[0].IsOk).IsTrue();

        // appended like assertz
        var index = Interpreter.LogSharpProgram.GetPredicateIndex("likes/2");
        Check.That(index.AllClauses.Last().ToString()).IsEqualTo("likes(bob, curry).");

        var answers = GetAnswers("likes(bob, What)?");
        Check.That(answers.Extracting(a => a.GetBinding("What").ToString()))
            .IsEquivalentTo(new List<string> { "pasta", "pizza", "curry" });
    }

    [Fact]
    public void AssertKeepsEvaluatingExpressionGoals()
    {
        // the parser wraps bare expressions ("X > 2") into assert/1 goals :
        // this legacy role must keep working next to clause assertion
        Interpreter.LoadProgram("big(X) :- X > 2.");
        Check.That(GetAnswers("big(3)?")[0].IsOk).IsTrue();
        Check.That(GetAnswers("big(1)?")[0].IsOk).IsFalse();
    }

    // -----------------------------------------------------------------------
    // asserting rules

    [Fact]
    public void AssertzCanAssertRulesWithBodies()
    {
        // assertz((edge(X) :- likes(X, _))) : the ":-" rule term is built
        // directly since the grammar cannot nest ":-" inside an argument
        var head = new Term("edge", new List<Valuable> { new Variable("X") });
        var body = new Term("likes", new List<Valuable> { new Variable("X"), new Variable("_") });
        var assertTerm = new Term("assertz", new List<Valuable>
        {
            new Term(":-", new List<Valuable> { head, body })
        });

        var answers = Interpreter.Search(new Question(assertTerm)).ToList();
        Check.That(answers).CountIs(1);
        Check.That(answers[0].IsOk).IsTrue();

        // the body of the asserted rule is executed on call
        Check.That(GetAnswers("edge(bob)?")[0].IsOk).IsTrue();
        Check.That(GetAnswers("edge(mary)?")[0].IsOk).IsTrue();
        Check.That(GetAnswers("edge(chris)?")[0].IsOk).IsFalse();
    }

    [Fact]
    public void AssertedRulesFlattenConjunctionBodies()
    {
        // both() :- counter(1), counter(2) with the body given as a ","/2 term
        var conjunction = new Term(",", new List<Valuable>
        {
            new Term("counter", new List<Valuable> { new Int(1) }),
            new Term("counter", new List<Valuable> { new Int(2) })
        });
        var assertTerm = new Term("assertz", new List<Valuable>
        {
            new Term(":-", new List<Valuable> { new Term("both"), conjunction })
        });

        var answers = Interpreter.Search(new Question(assertTerm)).ToList();
        Check.That(answers[0].IsOk).IsTrue();

        Check.That(GetAnswers("both()?")).CountIs(1);
        Check.That(GetAnswers("both()?")[0].IsOk).IsTrue();
    }

    // -----------------------------------------------------------------------
    // retract

    [Fact]
    public void RetractRemovesTheMatchingFactOnly()
    {
        var answers = GetAnswers("retract(likes(bob, pasta))?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0].IsOk).IsTrue();

        Check.That(GetAnswers("likes(bob, pasta)?")[0].IsOk).IsFalse();
        // everything else is untouched
        Check.That(GetAnswers("likes(bob, pizza)?")[0].IsOk).IsTrue();
        Check.That(GetAnswers("likes(mary, sushi)?")[0].IsOk).IsTrue();
    }

    [Fact]
    public void RetractEnumeratesEveryMatchingClause()
    {
        // one solution per matching clause, in database order
        var answers = GetAnswers("retract(counter(X))?");
        Check.That(answers).CountIs(2);
        Check.That(answers.Extracting(a => a.GetBinding("X").ToString()))
            .ContainsExactly("1", "2");

        // each solution removed its clause : the predicate is empty now
        Check.That(GetAnswers("counter(_)?")[0].IsOk).IsFalse();
    }

    [Fact]
    public void RetractFailsWhenNothingMatches()
    {
        var answers = GetAnswers("retract(likes(chris, food))?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0].IsOk).IsFalse();

        // the database was not touched
        Check.That(GetAnswers("likes(bob, pasta)?")[0].IsOk).IsTrue();
    }

    [Fact]
    public void RetractRespectsVariableBindingsOfThePattern()
    {
        // the pattern's first argument is bound : only the matching clause goes
        var answers = GetAnswers("retract(counter(1))?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0].IsOk).IsTrue();

        Check.That(GetAnswers("counter(1)?")[0].IsOk).IsFalse();
        Check.That(GetAnswers("counter(2)?")[0].IsOk).IsTrue();
    }

    [Fact]
    public void RetractKeepsTheIndexSynchronized()
    {
        GetAnswers("retract(likes(bob, pasta))?");

        // the clause was unlinked from the constant bucket and the fallback
        // list : a stale bucket would keep serving it through the index
        var index = Interpreter.LogSharpProgram.GetPredicateIndex("likes/2");
        Check.That(index.ConstantTable.ContainsKey("pasta")).IsFalse();
        Check.That(index.AllClauses.Count).IsEqualTo(2);

        Check.That(GetAnswers("likes(bob, pasta)?")[0].IsOk).IsFalse();
    }

    [Fact]
    public void RetractRemovesRulesThroughTheirHead()
    {
        Interpreter.LoadProgram("edge(X) :- likes(X, _).");
        Check.That(GetAnswers("edge(bob)?")[0].IsOk).IsTrue();

        // a plain term pattern matches the clause HEAD, facts and rules alike
        var answers = GetAnswers("retract(edge(X))?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0].IsOk).IsTrue();

        Check.That(GetAnswers("edge(bob)?")[0].IsOk).IsFalse();
        // likes/1... likes/2 clauses are untouched
        Check.That(GetAnswers("likes(bob, pasta)?")[0].IsOk).IsTrue();
    }

    [Fact]
    public void RetractCanRemoveRulesThroughHeadAndBodyPatterns()
    {
        Interpreter.LoadProgram("conjoint(X) :- counter(1) counter(X).");

        // retract((conjoint(X) :- Body)) : the body is unified too and the
        // body pattern variable receives the clause's body
        var head = new Term("conjoint", new List<Valuable> { new Variable("X") });
        var retractTerm = new Term("retract", new List<Valuable>
        {
            new Term(":-", new List<Valuable> { head, new Variable("Body") })
        });

        var answers = Interpreter.Search(new Question(retractTerm)).ToList();
        Check.That(answers).CountIs(1);
        Check.That(answers[0].IsOk).IsTrue();

        var body = answers[0].GetBinding("Body") as Term;
        Check.That(body).IsNotNull();
        // the two body goals folded back into a conjunction term
        Check.That(body.Name).IsEqualTo(",");
        Check.That(body.Parameters.Count).IsEqualTo(2);

        Check.That(GetAnswers("conjoint(1)?")[0].IsOk).IsFalse();
    }

    // -----------------------------------------------------------------------
    // retractall

    [Fact]
    public void RetractallRemovesEveryMatchingClause()
    {
        var answers = GetAnswers("retractall(likes(bob, _))?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0].IsOk).IsTrue();

        Check.That(GetAnswers("likes(bob, _)?")[0].IsOk).IsFalse();
        // other first arguments and other predicates are untouched
        Check.That(GetAnswers("likes(mary, sushi)?")[0].IsOk).IsTrue();
        Check.That(GetAnswers("counter(1)?")[0].IsOk).IsTrue();
    }

    [Fact]
    public void RetractallSucceedsEvenWhenNothingMatches()
    {
        var answers = GetAnswers("retractall(nothing(here))?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0].IsOk).IsTrue();

        answers = GetAnswers("retractall(likes(chris, _))?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0].IsOk).IsTrue();
    }

    [Fact]
    public void RetractallCanEmptyAPredicateCompletely()
    {
        GetAnswers("retractall(counter(_))?");
        Check.That(GetAnswers("counter(_)?")[0].IsOk).IsFalse();

        var index = Interpreter.LogSharpProgram.GetPredicateIndex("counter/1");
        Check.That(index.AllClauses).IsEmpty();
        Check.That(index.ConstantTable).IsEmpty();

        // the predicate is usable again through assertions
        GetAnswers("asserta(counter(7))?");
        Check.That(GetAnswers("counter(7)?")[0].IsOk).IsTrue();
    }

    // -----------------------------------------------------------------------
    // mutation safety with active choice points

    [Fact]
    public void RetractWhileChoicePointsAreActiveDoesNotBreakTheSearch()
    {
        // wipe/0 enumerates item/1 with choice points on all three clauses
        // while retract/1 removes them, then fails on purpose
        var answers = GetAnswers("wipe()?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0].IsOk).IsFalse();

        // every item clause was retracted and the engine is still consistent
        Check.That(GetAnswers("item(_)?")[0].IsOk).IsFalse();
        Check.That(Interpreter.LogSharpProgram.GetPredicateIndex("item/1").AllClauses).IsEmpty();
        Check.That(GetAnswers("likes(bob, pasta)?")[0].IsOk).IsTrue();
    }

    [Fact]
    public void AssertedClausesAreVisibleToLaterCallsOnly()
    {
        // a snapshot semantic : the enumeration of a predicate never grows
        // under its own feet, new clauses are served by the NEXT call
        Interpreter.LoadProgram("enumerate() :- item(_).");

        var answers = GetAnswers("enumerate()?");
        Check.That(answers).CountIs(3);

        GetAnswers("assertz(item(4))?");
        answers = GetAnswers("enumerate()?");
        Check.That(answers).CountIs(4);
    }

    [Fact]
    public void AssertAndRetractSurviveRecursiveQueries()
    {
        // dynamic stack : pop/1 retracts the head of the stack while
        // enumerating it through the index
        Interpreter.LoadProgram(@"
stack(top).
stack(middle).
stack(bottom).

popped(X) :- stack(X) retract(stack(X)).
");

        var answers = GetAnswers("popped(X)?");
        Check.That(answers).CountIs(3);
        Check.That(answers.Extracting(a => a.GetBinding("X").ToString()))
            .IsEquivalentTo(new List<string> { "top", "middle", "bottom" });
        Check.That(GetAnswers("stack(_)?")[0].IsOk).IsFalse();
    }
}
