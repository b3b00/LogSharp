First-argument indexing eliminates the $O(N)$ linear scan over a predicate's clauses during resolution. By inspecting the principal type and value of a goal's first argument, the engine can instantly narrow down candidate clauses, skipping entirely irrelevant branches of the database.1. The Core Dispatch StrategyIn logic programs, the first argument is empirically the most discriminative (e.g., distinguishing between an empty list [], a non-empty list [H|T], or specific atoms).When a goal like foo(Bar, Baz) is evaluated, the engine evaluates the first argument against a classification switch based on its runtime binding state:Variables (Unbound): Because a free variable can unify with anything, clauses whose first argument is a variable must always be candidates.Constants (Atoms, Numbers): Indexed via a hash map keyed directly on the constant value.Compound Terms: Indexed by their principal functor and arity (e.g., the list constructor ./2 or a custom functor node/3).2. Structuring the Predicate IndexInstead of storing a predicate as a flat List<Clause>, represent each predicate entry in your clause database as a specialized index structure:C#public class PredicateIndex {

```csharp
// Clauses whose first argument is a variable (must always be tried)

private readonly List<Clause> _variableClauses = new();

    // Exact match for atoms, numbers, and primitive constants
    private readonly Dictionary<object, List<Clause>> _constantTable = new();

    // Match for compound terms based on Functor/Arity (e.g., "."/2)
    private readonly Dictionary<FunctorArity, List<Clause>> _compoundTable = new();

    // Fallback ordered list for clauses where indexing isn't applicable
    private readonly List<Clause> _allClauses = new();
}
```

When a clause is added to the database (via static loading or assertz), your preprocessor or database manager inspects the term in the first argument position of the clause head and routes it:If it's a variable (Var or _), append to _variableClauses and _allClauses.If it's a constant (atom/number), insert into the bucket for that constant in _constantTable and append to _allClauses.If it's a compound term, extract its Functor/Arity key and insert into _compoundTable.3. The Lookup AlgorithmWhen resolving a goal, the engine must dereference the first argument before querying the index. If an argument points to a chain of bound variables, you must resolve it down to its ultimate binding or find that it is still free.
```csharp
public IEnumerable<Clause> GetCandidateClauses(Term goalFirstArg) {
// 1. Dereference the term to get its current binding state
Term derefArg = Dereference(goalFirstArg);

    if (derefArg is VariableTerm) {
        // If the first argument is uninstantiated, ALL clauses might match
        foreach (var c in _allClauses) yield return c;
        yield break;
    }

    // 2. Check constant index if it's a primitive
    if (derefArg is ConstantTerm constant) {
        if (_constantTable.TryGetValue(constant.Value, out var bucket)) {
            foreach (var c in bucket) yield return c;
        }
    }
    // 3. Check compound index if it's a structured term
    else if (derefArg is CompoundTerm compound) {
        var key = new FunctorArity(compound.Functor, compound.Arity);
        if (_compoundTable.TryGetValue(key, out var bucket)) {
            foreach (var c in bucket) yield return c;
        }
    }

    // 4. Always include variable-headed clauses as fallback
    foreach (var c in _variableClauses) {
        yield return c;
    }
}
```


## 4. Benchmarks and Performance Gains

See [`predicate_index_bench.md`](predicate_index_bench.md) for empirical benchmark results comparing indexed resolution with linear scanning.
