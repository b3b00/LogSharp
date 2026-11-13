using System.Diagnostics.CodeAnalysis;
using logSharp.model;

namespace logSharp.parser;

public class DcgNonTerminal : IDcgClause
{
    public Term Term { get; }
    public DcgNonTerminal(Term term) { Term = term; }
    
    [ExcludeFromCodeCoverage]
    public string Dump() => Term?.Dump();
}