using logSharp.model;

namespace logSharp.parser;

public class DcgNonTerminal : IDcgClause
{
    public Term Term { get; }
    public DcgNonTerminal(Term term) { Term = term; }
    public string Dump() => Term?.Dump();
}