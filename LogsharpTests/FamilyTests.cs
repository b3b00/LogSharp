using System.Collections.Generic;
using System.Linq;
using logSharp.interpretation;
using logSharp.interpretation.unification;
using logSharp.model;
using logSharp.parser;
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

cof(X,F) :- bag_of(X,father(F,X), L) member(X,L).";


        public FamilyTests() : base(PrologProgram)
        {
        }


       
        [Fact]
        public void FathersTest()
        {
            var answers = GetAnswers("father(X,Y)?");
            Assert.NotNull(answers);
            Assert.Equal(7, answers.Count);
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
            Assert.Single(answers);
            Assert.True(answers[0].IsOk);
            Assert.Empty(answers[0].Environment.Environment);

            answers = GetAnswers("grandpa(christian,X)?");
            Assert.NotNull(answers);
            Assert.Equal(4, answers.Count);
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
            Assert.NotNull(answers);
            Assert.True(answers[0].IsOk);
            Assert.Empty(answers[0].Environment.Environment);

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
    }
}