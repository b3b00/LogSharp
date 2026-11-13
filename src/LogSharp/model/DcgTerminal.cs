using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using logSharp.model;

namespace logSharp.parser;

public class DcgTerminal : IDcgClause
{
    public List<Valuable> Terminals { get; }
    public DcgTerminal(List<Valuable> terminals) { Terminals = terminals; }
    
    [ExcludeFromCodeCoverage]
    public string Dump() => $"[{string.Join(", ", Terminals.Select(t => t.Dump()))}]";
}