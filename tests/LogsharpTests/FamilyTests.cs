using System.Collections.Generic;
using System.Linq;
using logSharp.interpretation;
using logSharp.interpretation.unification;
using logSharp.model;
using logSharp.parser;
using NFluent;
using sly.lexer;
using sly.parser;
using sly.parser.generator;
using Xunit;
using Xunit.Sdk;

namespace ogsharpTests
{
    public class FamilyTests : BaseTest
    {

        public const string PrologProgram = @"
father(christian,olivier).
father(christian,frederic).
father(christian,julien).
father(frederic,lontzi).
father(frederic,eneko).
father(frederic,kepa).
father(julien,oihana).
grandpa(X,Z) :- father(X,Y) father(Y,Z).
gp(X,Y,Z) :- father(X,Y) father(Y,Z).
brother(X,Y) :- father(Z,X) father(Z,Y).

cof(C,F) :- bag_of(CC,father(F,CC), L) member(C,L).
children_count(F,N) :- bag_of(CC,father(F,CC), L) length(L,N).
sorted_children(F,S) :- bag_of(CC,father(F,CC), L) sort(L,S).
each_child(F,C) :- bag_of(CC,father(F,CC), L) length(L,3) member(C,L).";


        public FamilyTests() : base(PrologProgram)
        {
        }


       
        [Fact]
        public void FathersTest()
        {
            var answers = GetAnswers("father(X,Y)?");
            Check.That(answers).Not.IsNullOrEmpty();
            Check.That(answers).CountIs(7);
            var c = answers.Where(a =>
                    a.IsOk && a.Environment.ContainsVariable("X") && a.Environment["X"]?.ToString() == "christian")
                .ToList();
            Assert.Equal(3, c.Count);
            var f = answers.Where(a =>
                    a.IsOk && a.Environment.ContainsVariable("X") && a.Environment["X"]?.ToString() == "frederic")
                .ToList();
            Assert.Equal(3, f.Count);
            var j = answers.Where(a =>
                    a.IsOk && a.Environment.ContainsVariable("X") && a.Environment["X"]?.ToString() == "julien")
                .ToList();
            Assert.Single(j);
        }

        [Fact]
        public void GrandPaTest()
        {
            var answers = GetAnswers("grandpa(christian,oihana)?");
            Check.That(answers[0]).IsOk();

            answers = GetAnswers("grandpa(christian,X)?");
            Check.That(answers).Not.IsNullOrEmpty();
            Check.That(answers).CountIs(4);
            var grandchild = answers.Select(a => a.Environment["X"].ToString()).ToList();
            var shouldList = new List<string>() {"lontzi", "eneko", "kepa", "oihana"};
            foreach (var shouldItem in shouldList)
            {
                Assert.Contains(grandchild, x => x == shouldItem);
            }
        }

        [Fact]
        public void ChildOfTest()
        {
            var answers = GetAnswers("cof(christian,olivier)?");
            Check.That(answers).Not.IsNullOrEmpty();
            Check.That(answers[0]).IsOk();

            answers = GetAnswers("cof(X,christian)?");
            Assert.NotNull(answers);
            Assert.Equal(3, answers.Count);
            var children = answers.Select(a => a.Environment["X"].ToString()).ToList();
            var shouldList = new List<string>() {"frederic", "olivier", "julien"};
            foreach (var shouldItem in shouldList)
            {
                Assert.Contains(children, x => x == shouldItem);
            }
        }

        // Regression tests for the continuation re-link bug (see childofbug.md):
        // a binding produced by a built-in (bag_of, length, sort...) used as a
        // NON-final conjunct must survive into the next conjuncts, and the
        // remaining body goals must NOT be executed twice.

        [Fact]
        public void ChildrenCountTest()
        {
            // length/2 consumes the body-local list L produced by bag_of/3
            var answers = GetAnswers("children_count(christian,N)?");
            Check.That(answers).CountIs(1);
            Check.That(answers[0]).IsOk();
            Check.That(answers[0].GetBinding("N").ToString()).IsEqualTo("3");

            answers = GetAnswers("children_count(frederic,N)?");
            Check.That(answers).CountIs(1);
            Check.That(answers[0]).IsOk();
            Check.That(answers[0].GetBinding("N").ToString()).IsEqualTo("3");

            answers = GetAnswers("children_count(julien,N)?");
            Check.That(answers).CountIs(1);
            Check.That(answers[0]).IsOk();
            Check.That(answers[0].GetBinding("N").ToString()).IsEqualTo("1");
        }

        [Fact]
        public void SortedChildrenTest()
        {
            // sort/2 consumes the body-local list L produced by bag_of/3
            var answers = GetAnswers("sorted_children(christian,S)?");
            Check.That(answers).CountIs(1);
            Check.That(answers[0]).IsOk();
            var sorted = answers[0].GetBinding("S") as Term;
            Check.That(sorted).IsNotNull();
            Check.That(sorted.IsList).IsTrue();
            Check.That(string.Join(",", sorted.ToValuableList().Select(i => i.Dump())))
                .IsEqualTo("frederic,julien,olivier");
        }

        [Fact]
        public void EachChildThreeConjunctsTest()
        {
            // three conjuncts : bag_of binds L -> length consumes L -> member iterates L
            // julien has only 1 child, so length(L,3) must fail the whole rule.
            var answers = GetAnswers("each_child(julien,C)?");
            Check.That(answers[0]).Not.IsOk();

            answers = GetAnswers("each_child(christian,C)?");
            Check.That(answers).CountIs(3);
            var children = answers.Select(a => a.Environment["C"].ToString()).ToList();
            Check.That(children.Extracting(x => x))
                .IsEquivalentTo("frederic", "julien", "olivier");
        }
    }
}