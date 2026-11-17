using System.Collections.Generic;
using System.Linq;
using logSharp.interpretation;
using logSharp.interpretation.unification;
using logSharp.model;
using NFluent;
using Xunit;

namespace LogsharpTests;

public class PredicateIndexTests : BaseTest
{
    public PredicateIndexTests() : base("")
    {
    }

    [Fact]
    public void FunctorArity_PropertiesAndEquality()
    {
        var fa1 = new FunctorArity("foo", 2);
        var fa2 = new FunctorArity("foo", 2);
        var fa3 = new FunctorArity("foo", 3);
        var fa4 = new FunctorArity("bar", 2);

        Check.That(fa1.Functor).IsEqualTo("foo");
        Check.That(fa1.Arity).IsEqualTo(2);
        Check.That(fa1).IsEqualTo(fa2);
        Check.That(fa1 == fa2).IsTrue();
        Check.That(fa1 == fa3).IsFalse();
        Check.That(fa1 == fa4).IsFalse();
        Check.That(fa1.GetHashCode()).IsEqualTo(fa2.GetHashCode());
        Check.That(fa1.ToString()).IsEqualTo("foo/2");
    }

    [Fact]
    public void PredicateIndex_Empty()
    {
        var index = new PredicateIndex();
        Check.That(index.AllClauses).IsEmpty();
        Check.That(index.VariableClauses).IsEmpty();
        Check.That(index.ConstantTable).IsEmpty();
        Check.That(index.CompoundTable).IsEmpty();

        // Null clause addition is safely ignored
        index.Add(null);
        Check.That(index.AllClauses).IsEmpty();
    }

    [Fact]
    public void PredicateIndex_ZeroArityClauses()
    {
        var index = new PredicateIndex();
        var rule1 = new Fact("zero_arity");
        index.Add(rule1);

        Check.That(index.AllClauses).ContainsExactly(rule1);
        Check.That(index.VariableClauses).IsEmpty();
        Check.That(index.ConstantTable).IsEmpty();
        Check.That(index.CompoundTable).IsEmpty();

        // Querying with null (0-arity goal) returns all clauses
        var candidates = index.GetCandidateClauses((Valuable)null).ToList();
        Check.That(candidates).ContainsExactly(rule1);
    }

    [Fact]
    public void PredicateIndex_VariableHeadClauses()
    {
        var index = new PredicateIndex();
        var ruleVar = new Fact("p", new List<Valuable> { new Variable("X") });
        var ruleDiscard = new Fact("p", new List<Valuable> { new Variable("_") });

        index.Add(ruleVar);
        index.Add(ruleDiscard);

        Check.That(index.VariableClauses).ContainsExactly(ruleVar, ruleDiscard);
        Check.That(index.AllClauses).ContainsExactly(ruleVar, ruleDiscard);
        Check.That(index.ConstantTable).IsEmpty();
        Check.That(index.CompoundTable).IsEmpty();
    }

    [Fact]
    public void PredicateIndex_ConstantClauses()
    {
        var index = new PredicateIndex();
        var ruleAtom = new Fact("p", new List<Valuable> { new Atom("alpha") });
        var ruleInt = new Fact("p", new List<Valuable> { new Int(42) });
        var ruleDouble = new Fact("p", new List<Valuable> { new Double(3.14) });
        var ruleStr = new Fact("p", new List<Valuable> { new String("hello") });

        index.Add(ruleAtom);
        index.Add(ruleInt);
        index.Add(ruleDouble);
        index.Add(ruleStr);

        Check.That(index.AllClauses).CountIs(4);
        Check.That(index.VariableClauses).IsEmpty();

        // Direct table inspection
        Check.That(index.ConstantTable.ContainsKey("alpha")).IsTrue();
        Check.That(index.ConstantTable["alpha"]).ContainsExactly(ruleAtom);

        Check.That(index.ConstantTable.ContainsKey(42)).IsTrue();
        Check.That(index.ConstantTable[42]).ContainsExactly(ruleInt);

        Check.That(index.ConstantTable.ContainsKey(3.14)).IsTrue();
        Check.That(index.ConstantTable[3.14]).ContainsExactly(ruleDouble);

        Check.That(index.ConstantTable.ContainsKey("hello")).IsTrue();
        Check.That(index.ConstantTable["hello"]).ContainsExactly(ruleStr);
    }

    [Fact]
    public void PredicateIndex_CompoundClauses()
    {
        var index = new PredicateIndex();
        var ruleEmptyList = new Fact("p", new List<Valuable> { Term.EmptyListTerm() });
        var ruleConsList = new Fact("p", new List<Valuable> {
            Term.ListTerm(new Atom("h"), new Variable("T"))
        });
        var ruleCompound = new Fact("p", new List<Valuable> {
            new Term("node", new List<Valuable> { new Atom("left"), new Atom("right") })
        });

        index.Add(ruleEmptyList);
        index.Add(ruleConsList);
        index.Add(ruleCompound);

        Check.That(index.AllClauses).CountIs(3);
        Check.That(index.VariableClauses).IsEmpty();

        var list0 = new FunctorArity("list", 0);
        var list2 = new FunctorArity("list", 2);
        var node2 = new FunctorArity("node", 2);

        Check.That(index.CompoundTable.ContainsKey(list0)).IsTrue();
        Check.That(index.CompoundTable[list0]).ContainsExactly(ruleEmptyList);

        Check.That(index.CompoundTable.ContainsKey(list2)).IsTrue();
        Check.That(index.CompoundTable[list2]).ContainsExactly(ruleConsList);

        Check.That(index.CompoundTable.ContainsKey(node2)).IsTrue();
        Check.That(index.CompoundTable[node2]).ContainsExactly(ruleCompound);
    }

    [Fact]
    public void PredicateIndex_Dereference()
    {
        var index = new PredicateIndex();

        // Null
        Check.That(index.Dereference(null)).IsNull();

        // Constants and terms remain unchanged
        var atom = new Atom("a");
        var intVal = new Int(10);
        var term = new Term("node");
        Check.That(index.Dereference(atom)).IsEqualTo(atom);
        Check.That(index.Dereference(intVal)).IsEqualTo(intVal);
        Check.That(index.Dereference(term)).IsEqualTo(term);

        // Unbound variable
        var varX = new Variable("X");
        Check.That(index.Dereference(varX)).IsEqualTo(varX);

        var unif = new Unification();
        Check.That(index.Dereference(varX, unif)).IsEqualTo(varX);

        // Single binding: X -> Atom("b")
        unif.SetBinding(varX, new Atom("b"));
        Check.That(index.Dereference(varX, unif)).IsEqualTo(new Atom("b"));

        // Chained bindings: A -> B -> C -> Int(99)
        var varA = new Variable("A");
        var varB = new Variable("B");
        var varC = new Variable("C");
        var targetInt = new Int(99);

        unif.SetBinding(varA, varB);
        unif.SetBinding(varB, varC);
        unif.SetBinding(varC, targetInt);

        Check.That(index.Dereference(varA, unif)).IsEqualTo(targetInt);

        // Chained bindings stopping at unbound variable: P -> Q -> R
        var varP = new Variable("P");
        var varQ = new Variable("Q");
        var varR = new Variable("R");
        unif.SetBinding(varP, varQ);
        unif.SetBinding(varQ, varR);

        Check.That(index.Dereference(varP, unif)).IsEqualTo(varR);

        // Circular reference guard: X -> Y -> X
        var varSelf = new Variable("Self");
        unif.SetBinding(varSelf, varSelf);
        Check.That(index.Dereference(varSelf, unif)).IsEqualTo(varSelf);
    }

    [Fact]
    public void PredicateIndex_GetCandidateClauses_DispatchScenarios()
    {
        var index = new PredicateIndex();

        var ruleA = new Fact("p", new List<Valuable> { new Atom("a") });
        var ruleB = new Fact("p", new List<Valuable> { new Atom("b") });
        var rule42 = new Fact("p", new List<Valuable> { new Int(42) });
        var ruleNil = new Fact("p", new List<Valuable> { Term.EmptyListTerm() });
        var ruleList = new Fact("p", new List<Valuable> { Term.ListTerm(new Variable("H"), new Variable("T")) });
        var ruleNode = new Fact("p", new List<Valuable> { new Term("node", new List<Valuable> { new Variable("X") }) });
        var ruleFallback = new Fact("p", new List<Valuable> { new Variable("Fallback") });

        index.Add(ruleA);
        index.Add(ruleB);
        index.Add(rule42);
        index.Add(ruleNil);
        index.Add(ruleList);
        index.Add(ruleNode);
        index.Add(ruleFallback);

        // 1. Unbound variable queries ALL clauses
        var unboundCandidates = index.GetCandidateClauses(new Variable("V")).ToList();
        Check.That(unboundCandidates).ContainsExactly(ruleA, ruleB, rule42, ruleNil, ruleList, ruleNode, ruleFallback);

        // 2. Atom match: returns bucket for 'a' + variable fallback
        var atomCandidates = index.GetCandidateClauses(new Atom("a")).ToList();
        Check.That(atomCandidates).ContainsExactly(ruleA, ruleFallback);

        // 3. Int match: returns bucket for 42 + variable fallback
        var intCandidates = index.GetCandidateClauses(new Int(42)).ToList();
        Check.That(intCandidates).ContainsExactly(rule42, ruleFallback);

        // 4. Non-matching constant: returns ONLY variable fallback
        var nonMatchCandidates = index.GetCandidateClauses(new Atom("unknown")).ToList();
        Check.That(nonMatchCandidates).ContainsExactly(ruleFallback);

        // 5. Empty list match: returns bucket for list/0 + variable fallback
        var nilCandidates = index.GetCandidateClauses(Term.EmptyListTerm()).ToList();
        Check.That(nilCandidates).ContainsExactly(ruleNil, ruleFallback);

        // 6. Non-empty list match: returns bucket for list/2 + variable fallback
        var listCandidates = index.GetCandidateClauses(Term.ListTerm(new Int(1), Term.EmptyListTerm())).ToList();
        Check.That(listCandidates).ContainsExactly(ruleList, ruleFallback);

        // 7. Compound match: returns bucket for node/1 + variable fallback
        var nodeCandidates = index.GetCandidateClauses(new Term("node", new List<Valuable> { new Atom("val") })).ToList();
        Check.That(nodeCandidates).ContainsExactly(ruleNode, ruleFallback);

        // 8. Compound non-matching arity (node/2): returns ONLY variable fallback
        var nodeArityMismatch = index.GetCandidateClauses(new Term("node", new List<Valuable> { new Atom("1"), new Atom("2") })).ToList();
        Check.That(nodeArityMismatch).ContainsExactly(ruleFallback);

        // 9. Bound variable dereferenced through unification:
        var unif = new Unification();
        unif.SetBinding(new Variable("Bound"), new Atom("b"));
        var boundCandidates = index.GetCandidateClauses(new Variable("Bound"), unif).ToList();
        Check.That(boundCandidates).ContainsExactly(ruleB, ruleFallback);
    }

    [Fact]
    public void LogSharpProgram_MaintainsIndexes()
    {
        var rule1 = new Fact("foo", new List<Valuable> { new Atom("bar") });
        var prog = new LogSharpProgram(new List<Rule> { rule1 });

        Check.That(prog.PredicateIndexes.ContainsKey("foo/1")).IsTrue();
        var index = prog.GetPredicateIndex("foo/1");
        Check.That(index).IsNotNull();
        Check.That(index.AllClauses).ContainsExactly(rule1);

        // Append rule
        var rule2 = new Fact("foo", new List<Valuable> { new Atom("baz") });
        prog.Append(rule2);

        Check.That(index.AllClauses).ContainsExactly(rule1, rule2);

        // Candidate rules through program
        var candidates = prog.GetCandidateRules("foo/1", new Atom("baz")).ToList();
        Check.That(candidates).ContainsExactly(rule2);

        // Unknown spec falls back gracefully
        var unknown = prog.GetCandidateRules("nonexistent/0").ToList();
        Check.That(unknown).IsEmpty();
    }

    [Fact]
    public void EndToEnd_FirstArgumentIndexingResolution()
    {
        string program = @"
item(apple, fruit, red).
item(banana, fruit, yellow).
item(carrot, vegetable, orange).
item(date, fruit, brown).
item(eggplant, vegetable, purple).
";
        Interpreter.LoadProgram(program);

        // Querying for specific first argument
        var answers = GetAnswers("item(banana, Cat, Col)?");

        Check.That(answers).CountIs(1);
        Check.That(answers[0].IsOk).IsTrue();
        Check.That(answers[0].GetBinding("Cat").ToString()).IsEqualTo("fruit");
        Check.That(answers[0].GetBinding("Col").ToString()).IsEqualTo("yellow");

        // Querying for non-existing first argument
        var noAnswers = GetAnswers("item(spinach, Cat, Col)?");

        Check.That(noAnswers).CountIs(1);
        Check.That(noAnswers[0].IsOk).IsFalse();

        // Querying with unbound first argument enumerates all
        var allAnswers = GetAnswers("item(Name, Cat, Col)?");

        Check.That(allAnswers).CountIs(5);
        var names = allAnswers.Select(a => a.GetBinding("Name").ToString()).ToList();
        Check.That(names).IsEquivalentTo("apple", "banana", "carrot", "date", "eggplant");
    }

    [Fact]
    public void EndToEnd_IndexingWithRecursiveListPredicates()
    {
        string program = @"
has_item(X, [X | _]).
has_item(X, [_ | Tail]) :- has_item(X, Tail).
";
        Interpreter.LoadProgram(program);

        // Querying with empty list fails directly (no clauses match list/0)
        var emptyAnswers = GetAnswers("has_item(a, [])?");

        Check.That(emptyAnswers).CountIs(1);
        Check.That(emptyAnswers[0].IsOk).IsFalse();

        var foundAnswers = GetAnswers("has_item(b, [a, b, c])?");
        Check.That(foundAnswers.Any(a => a.IsOk)).IsTrue();
    }

    [Fact]
    public void LargeDatabase_FirstArgumentIndexingIsFast()
    {
        const int factCount = 2000;
        var rules = new List<Rule>(factCount);
        for (int i = 0; i < factCount; i++)
        {
            rules.Add(new Fact("kv", new List<Valuable> {
                new Atom($"key_{i}"),
                new Int(i * 10)
            }));
        }

        var prog = new LogSharpProgram(rules);
        var index = prog.GetPredicateIndex("kv/2");
        Check.That(index).IsNotNull();
        Check.That(index.AllClauses.Count).IsEqualTo(factCount);
        Check.That(index.ConstantTable.Count).IsEqualTo(factCount);

        // Candidate lookup takes only the matching clause
        var candidates = prog.GetCandidateRules("kv/2", new Atom("key_1234")).ToList();
        Check.That(candidates).CountIs(1);
        Check.That((candidates[0].Parameters[1] as Int).Value).IsEqualTo(12340);

        // Candidate lookup for non-existent key returns 0 candidates
        var nonCandidates = prog.GetCandidateRules("kv/2", new Atom("non_existent")).ToList();
        Check.That(nonCandidates).IsEmpty();
    }
}
