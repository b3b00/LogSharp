# Bug: `ChildOfTest` — a bound `bag_of/3` output never reaches the next conjunct

Rule under test (`tests/LogsharpTests/FamilyTests.cs`):

```prolog
cof(C,F) :- bag_of(CC,father(F,CC), L) member(C,L).
```

Symptom: `cof(christian,olivier)?` and `cof(X,christian)?` returned *"don't know."*
It *looked* like `bag_of(CC,father(F,CC),L)` failed to pass the bound `L` to the
next clause `member(C,L)`. That intuition was right about the symptom, but the
root cause lies **not in `bag_of`** — it lies in how the interpreter re-links a
*continuation goal* to its parent when the continuation completes, in
`Interpreter.Search`.

## Execution trace (before the fix)

1. `bag_of(CC,father(F,CC),L)` runs as a built-in inside the rule
   (`Interpreter.cs`, built-in dispatch). It correctly binds
   `L → [olivier, frederic, julien]`, then creates a **continuation goal**
   (`Interpreter.cs:233`):

   ```csharp
   Goal built = new Goal(current.Rule, current, builtInUnification, false);
   built.Unification.AddBindings(current.Unification);   // {L=[...]} + parent env
   built.Index  = current.Index + 1;                     // next body goal: member
   ```

   So far so good — the continuation inherits the full substitution, including `L`.

2. The continuation runs `member(C,L)` at its own `Index = 1`.
   **This works.** `member` binds `C` to each child and pushes 7 successor
   continuations.

3. When a continuation finishes (`Index >= Count`), `Search` re-links it to its
   parent (`Interpreter.cs:189-196`):

   ```csharp
   var destination = current.IsContinuation ? p.Head : p.Goals[p.Index].Head;
   u = Unifier.UnifyTerms(current.Head, current.Unification,
                          destination, p.Unification);
   ```

   `current.Head` / `p.Head` is the **rule head** `cof(C,F)` — only 2 parameters.
   `UnifyTerms` walks only the source term's parameters, so body-local variables
   such as `__goal0_L` are **structurally invisible**: they are dropped from the
   merged substitution `u`. The parent inherits only the head bindings.

4. Worse, the parent was re-pushed with `p.Index++`. But a continuation has
   **already executed all of the parent's remaining goals** — so the parent was
   re-pushed *back at* goal `member(C,L)`, which ran a **second time**. This
   time `L` was unbound (dropped in step 3), so `member/2` fell through to
   `Unification.Failed()` (`PrologBuiltins.cs:56`). All 7 branches died and the
   query returned *"don't know."*

Verbose trace of the failure (the two smoking guns):

```
calling built-in bag_of/3 ... >>> yes.  [ __goal0_L = [oihana, kepa, eneko, lontzi, julien, frederic, olivier] ]
... member produces 7 × ">>> yes. [ __goal0_C = X ]"  (all succeed) ...
stck childrenOf(...) @ 2
pop  childrenOf(...) @ 2     (continuation completes → re-link to parent)
stck childrenOf(...) @ 1     (parent re-pushed at Index 1 — re-running member!)
calling built-in member/2 member(__goal0_C, __goal0_L) @ 0
        >>> don't know.       ← __goal0_L is gone from the substitution
```

So it is a **double bug** in the completion path:

| Bug | Effect |
|-----|--------|
| Lost substitution — re-link unifies only the head parameters | Body-local bindings (e.g. `L` built by `bag_of/3`) are dropped |
| Impossible backtracking position — `p.Index++` | Goals the continuation already executed get re-executed, now with lost bindings |

## The fix

### 1. `src/LogSharp/interpretation/Interpreter.cs` — continuation completion

When a continuation completes, the parent now inherits the **whole**
continuation substitution (not only head-parameter bindings) and is re-pushed
**past** the goals the continuation already executed:

```csharp
if (current.IsContinuation)
{
    // A continuation goal has already executed ALL the remaining goals
    // of its parent (from Index+1 to Count). The parent must be re-pushed
    // past those goals (at Count), NOT at Index+1 which would re-execute
    // them. Also, the parent must inherit the whole continuation
    // unification, not only the head-parameter bindings returned by
    // UnifyTerms, otherwise body-local bindings (like the list built by
    // bag_of/3) are lost for the next conjuncts.
    var merged = new Unification();
    merged.AddBindings(p.Unification);
    merged.AddBindings(current.Unification);
    merged.AddBindings(u);
    p.Unification = merged;
    p.Index = current.Count;
}
else                       // normal rule-goal completion: unchanged
{
    p.Index++;
    p.Unification = u;
}
```

The `else` branch keeps the exact previous behavior for facts and regular
rule-goal completions, so `FathersTest`, `GrandPaTest` and everything else are
untouched.

### 2. `src/LogSharp/builtins/PrologBuiltins.cs` — `fullyFailed` detection in `BagOf`

The old check was mathematically wrong:

```csharp
bool fullyFailed = results.Select(r => r.IsOk)
    .Aggregate(false, (bool b1, bool b2) => !b1 && !b2);
```

For a *mixed* result set `[ok, failed]` it computes `!ok && !failed = false`,
then `!false && !false = true` — i.e. *"all failed"* — and wrongly declares the
whole `bag_of` a failure even though solutions exist. Replaced with:

```csharp
bool fullyFailed = !results.Any(r => r != null && r.IsOk);
```

(Both occurrences — the bound-variable branch and the bound-list branch.)

## Scope of the fix

`bag_of` was only the messenger. Every "generating" built-in that is used as a
non-final conjunct shares the same completion path, so `map_list/3`,
`length/2` and `sort/2` chained after another conjunct were silently broken by
the same re-link bug — this single interpreter fix repairs all of them.

Note: the built-ins work fine at the *top level* of a query because
`current.Rule == null` there and the results are returned as-is
(`Interpreter.cs:243-250`), with no continuation and no parent re-link — that
is why `bag_of(X,relation(a,X),L)?` passed in `PrologBuiltinsTests` while the
mid-conjunction usage failed.

## Result

`ChildOfTest` passes; the full test suite is green (38/38).
