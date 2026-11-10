using System.Collections.Generic;
using System.Linq;
using logSharp.model;

namespace logSharp.parser;

public class DcgPrologGoals : IDcgClause
{
    public List<Term> Goals { get; }
    public DcgPrologGoals(List<Term> goals) { Goals = goals; }
    public string Dump() => $"{{{string.Join(", ", Goals.Select(g => g.Dump()))}}}";
}