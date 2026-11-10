using System.Collections.Generic;
using logSharp.model;

namespace logSharp.parser;

public class DcgTranslator
{
    public static void AddPrologGoal(List<Term> list, LogSharpAST ast)
    {
        if (ast is Term t)
        {
            list.Add(t);
        }
        else if (ast is Atom a)
        {
            list.Add(new Term(a.Name));
        }
    }

    public static Valuable BuildListWithTail(List<Valuable> items, Valuable tail)
    {
        Valuable current = tail;
        for (int i = items.Count - 1; i >= 0; i--)
        {
            current = model.Term.ListTerm(items[i], current);
        }
        return current;
    }

    public static void CollectVariables(LogSharpAST ast, HashSet<string> vars)
    {
        if (ast == null) return;
        if (ast is Variable v)
        {
            if (!v.IsDiscardableVariable)
            {
                vars.Add(v.Name);
            }
        }
        else if (ast is Term t)
        {
            if (t.Parameters != null)
            {
                foreach (var p in t.Parameters)
                {
                    CollectVariables(p, vars);
                }
            }
        }
        else if (ast is BinaryOperation bin)
        {
            CollectVariables(bin.LeftExpresion, vars);
            CollectVariables(bin.RightExpression, vars);
        }
        else if (ast is UnaryOperation un)
        {
            CollectVariables(un.Expression, vars);
        }
    }

    public LogSharpAST TranslateDcg(LogSharpAST headAst, List<IDcgClause> dcgClauses)
    {
        Term head = headAst is Term t ? t : (headAst is Atom a ? new Term(a.Name) : null);
        if (head == null) return null;

        var usedVarNames = new HashSet<string>();
        CollectVariables(head, usedVarNames);
        foreach (var clause in dcgClauses)
        {
            if (clause is DcgNonTerminal nt)
            {
                CollectVariables(nt.Term, usedVarNames);
            }
            else if (clause is DcgTerminal term)
            {
                foreach (var val in term.Terminals)
                {
                    CollectVariables(val, usedVarNames);
                }
            }
            else if (clause is DcgPrologGoals pg)
            {
                foreach (var goal in pg.Goals)
                {
                    CollectVariables(goal, usedVarNames);
                }
            }
        }

        int varCounter = 0;
        string NextVar()
        {
            while (true)
            {
                string name = $"S{varCounter++}";
                if (!usedVarNames.Contains(name))
                {
                    usedVarNames.Add(name);
                    return name;
                }
            }
        }

        var nonTerminals = new List<DcgNonTerminal>();
        foreach (var c in dcgClauses)
        {
            if (c is DcgNonTerminal nt)
            {
                nonTerminals.Add(nt);
            }
        }

        int p = nonTerminals.Count;

        var segmentTerminals = new List<List<Valuable>>();
        for (int i = 0; i <= p; i++)
        {
            segmentTerminals.Add(new List<Valuable>());
        }

        int currentNtIndex = 0;
        foreach (var c in dcgClauses)
        {
            if (c is DcgTerminal term)
            {
                segmentTerminals[currentNtIndex].AddRange(term.Terminals);
            }
            else if (c is DcgNonTerminal)
            {
                currentNtIndex++;
            }
        }

        Valuable headIn;
        Variable sOut;
        var ntInStreams = new Valuable[p];
        var ntOutStreams = new Valuable[p];

        if (p == 0)
        {
            sOut = new Variable(NextVar());
            headIn = BuildListWithTail(segmentTerminals[0], sOut);
        }
        else
        {
            if (segmentTerminals[0].Count > 0)
            {
                var nt0InVar = new Variable(NextVar());
                ntInStreams[0] = nt0InVar;
                headIn = BuildListWithTail(segmentTerminals[0], nt0InVar);
            }
            else
            {
                var sIn = new Variable(NextVar());
                ntInStreams[0] = sIn;
                headIn = sIn;
            }

            for (int i = 1; i < p; i++)
            {
                var nextInVar = new Variable(NextVar());
                ntInStreams[i] = nextInVar;
                ntOutStreams[i - 1] = BuildListWithTail(segmentTerminals[i], nextInVar);
            }

            sOut = new Variable(NextVar());
            ntOutStreams[p - 1] = BuildListWithTail(segmentTerminals[p], sOut);
        }

        var bodyClauses = new List<Term>();
        int ntCursor = 0;
        foreach (var c in dcgClauses)
        {
            if (c is DcgPrologGoals pg)
            {
                bodyClauses.AddRange(pg.Goals);
            }
            else if (c is DcgNonTerminal nt)
            {
                var ntParams = new List<Valuable>(nt.Term.Parameters);
                ntParams.Add(ntInStreams[ntCursor]);
                ntParams.Add(ntOutStreams[ntCursor]);
                bodyClauses.Add(new Term(nt.Term.Name, ntParams));
                ntCursor++;
            }
        }

        var headParams = new List<Valuable>(head.Parameters);
        headParams.Add(headIn);
        headParams.Add(sOut);
        var headWithDiffList = new Term(head.Name, headParams);

        if (bodyClauses.Count == 0)
        {
            return new Fact(headWithDiffList);
        }
        return new Predicate(headWithDiffList, bodyClauses);
    }
}