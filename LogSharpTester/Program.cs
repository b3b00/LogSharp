using System;
using logSharp;
using logSharp.helpers;
using logSharp.interpretation.unification;
using logSharp.model;
using LogSharpTester;

namespace Tester
{
    class Program
    {

        static void RecursiveUnificationTest()
        {
//            unify(member_of(X, L),[ X = c , L = [b, c] , member_of(X, [_, L]),[ X = c , L = [b, c] ] <-- should be [_,L?]
// Goal child = new Goal(applicable, current, current.Unification??);
            var unifier = new NestedTermUnifier();
            Term term1 = new Term("member_of", ListHelper.BuildList<Valuable>(new Variable("X"), new Variable("L")));
            Unification u1 = new Unification();
            u1.AddBinding("X",new Atom("c") );
            u1.AddBinding("L",Term.ListTerm(ListHelper.BuildList<Valuable>(new Atom("b"),new Atom("c") )));


            var l = Term.ListTerm(
                new Variable("_"),
                new Variable("L")
                );
            Term child = new Term("member_of",
                                    ListHelper.BuildList<Valuable>(
                                        new Variable("X"),
                                        l
                                        )
                                 );
            
            var childU = new Unification();
            var emptyChildU = new Unification();
            
            childU.AddBinding("X",new Atom("c") );
            childU.AddBinding("L",Term.ListTerm(ListHelper.BuildList<Valuable>(new Atom("b"),new Atom("c") )));

            var res = unifier.UnifyTerms(term1, u1, child, emptyChildU);
            ;




        }
        
        static void Main(string[] args)
        {
            PrologRepl.Loop(args);
            RecursiveUnificationTest();
        }
    }
}