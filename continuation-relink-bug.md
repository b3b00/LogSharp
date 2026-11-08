# Builtin continuation re-link bug: lost substitution + double execution

> Original file `childofbug.md`, renamed because the bug is **not** specific to
> `bag_of` or the `cof/2` family rule — it lives in the interpreter's goal
> search loop and affects *every* built-in used as a non-final conjunct.

## TL;DR

In `Interpreter.Search`, when a **continuation goal** (created to keep evaluating
the rest of a rule body after a built-in ran) completes, it is re-linked to its
parent goal in a way that:

1. **drops body-local variable bindings** — the parent only inherits bindings
   for the *rule head* parameters, so any variable the built-in bound inside the
   body (e.g. the list `L` built by `bag_of/3`) is lost, and
2. **re-executes the remaining body goals** — the parent is re-pushed at
   `Index+1`, but a continuation has *already* executed goals `Index+1..Count`,
   so every remaining goal runs a second time, now on top of the lost bindings.

Symptom shape (the failing pattern — `cof/2` from the family tests):

```prolog
cof(C,F) :- bag_of(CC,father(F,CC), L) member(C,L).
```

`bag_of` binds `L` correctly, `member(C,L)` iterates it correctly, and then the
re-link erases `L`; `member` is re-run with an unbound `L` and everything dies.
`bag_of` was only the messenger.

## The failing pattern in general terms

Any rule whose body is

```prolog
r(...) :- builtin(..., V) ...anything using V...
```

where `V` is a **body-local variable** (it does not appear in the rule head).
If `V` appears in the rule head, the bug is *masked* (see below), which is why
several existing built-in tests passed while `ChildOfTest` failed.

## Root cause (`src/LogSharp/interpretation/Interpreter.cs`)

Built-ins inside a rule body are executed by creating a *continuation goal*
(`Interpreter.Search`, built-in dispatch, `Goal(current.Rule, current, ...,
freshenVariables=false)`) whose unification is built from the built-in result
plus the parent's bindings. Field notes on the execution trace:

```
calling built-in bag_of/3 ... >>> yes.  [ __goal0_L = [oihana, kepa, eneko, lontzi, julien, frederic, olivier] ]
... member produces 7 × ">>> yes. [ __goal0_C = X ]"  (all succeed)
stck childrenOf(...) @ 2
pop  childrenOf(...) @ 2     (continuation completes -> re-link to parent)
stck childrenOf(...) @ 1     (parent re-pushed at Index 1 -- re-running member!)
calling built-in member/2 member(__goal0_C, __goal0_L) @ 0
        >>> don't know.       <- __goal0_L is gone from the substitution
```

The completion path did:

```csharp
var destination = current.IsContinuation ? p.Head : p.Goals[p.Index].Head;
u = Unifier.UnifyTerms(current.Head, current.Unification,
                       destination, p.Unification);
if (u.IsOk)
{
    p.Index++;                 // BUG 2: re-executes what the continuation already ran
    p.Unification = u;         // BUG 1: head-only unification, body-local bindings dropped
    stack.Push(p);
}
```

Two distinct bugs:

| Bug | Effect |
|-----|--------|
| **Lost substitution** — `UnifyTerms` walks only the source term's *parameters*; `current.Head`/`p.Head` is the rule head | Body-local bindings (e.g. `L`) are structurally invisible and dropped |
| **Impossible backtracking position** — `p.Index++` | Goals the continuation already executed get re-executed on those broken bindings |

### Why it looked variable-dependent

If the carried variable **appears in the rule head**, the head-only unification
happens to transfer it (it *is* a head parameter), so rules like

```prolog
children_count(F,N) :- bag_of(CC,father(F,CC), L) length(L,N).
```

used to "work" even though the re-execution happened — `length/2` re-run with a
free `L` and a bound `N` silently re-binds `L` to freshly generated variables.
Only **body-local** bindings actually break. This nuance decides which of the
regression tests below discriminate the old code.

## The fix (`src/LogSharp/interpretation/Interpreter.cs`)

When a continuation completes, the parent now inherits the **whole**
continuation substitution and skips everything the continuation already ran:

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
rule-goal completions.

## Which built-ins are affected

Every built-in goes through the same continuation path (`Interpreter.Search`
built-in dispatch), so **any** built-in used as a non-final conjunct is exposed
to this bug, in either of two ways:

- **bindings lost** — when it binds a body-local variable consumed by a later
  conjunct: `bag_of/3`, `map_list/3`, `length/2` (list generation), `sort/2`,
  and any future built-in,
- **double execution** — even when nothing is bound, the remaining body ran
  twice; that can duplicate answers, change rule-body side-effecting queries or
  deserialize into wrongful failures, depending on how the re-executed built-in
  resolves with lost/free bindings.

The two built-in "meta" families are different: `call/N` and `not/N` *return
their nested search's complete environments* (that is also why they survived),
whereas `bag_of`, `map_list`, `length`, `sort` synthesize a **fresh unification
holding a single new binding** — exactly the binding shape the old re-link
destroyed outside the head.

### Impact in the prelude

`src/LogSharp/prelude/prelude.pro` :

```prolog
distinct_elements_fast(List) :-
    sort(List, Sorted)
    length(List, Len)
    length(Sorted, Len).
```

`Sorted` is produced by `sort/2` and consumed by the last `length/2` conjunct —
a body-local binding flowing between two built-ins, i.e. the broken shape;

## Secondary fix (`src/LogSharp/builtins/PrologBuiltins.cs`, `BagOf`)

The `fullyFailed` detection was also mathematically wrong:

```csharp
// before : .Aggregate(false, (b1,b2) => !b1 && !b2));
//          for [ok, failed] this computes true ("all failed") although
//          solutions exist
bool fullyFailed = !results.Any(r => r != null && r.IsOk);   // after
```

(Both occurrences: bound-variable branch and bound-list branch.)

## Regression tests (added)

In `tests/LogsharpTests/FamilyTests.cs` (program extended with):

```prolog
children_count(F,N) :- bag_of(CC,father(F,CC), L) length(L,N).
sorted_children(F,S) :- bag_of(CC,father(F,CC), L) sort(L,S).
each_child(F,C) :- bag_of(CC,father(F,CC), L) length(L,3) member(C,L).
```

| Test | What it pins | Fails on pre-fix code? |
|------|--------------|------------------------|
| `ChildOfTest` (original) | `bag_of` binding consumed by `member` | **yes** |
| `ChildrenCountTest` | `length/2` consuming `bag_of`'s list (1/3/1 children) | no (head-var masking) |
| `SortedChildrenTest` | `sort/2` consuming `bag_of`'s list | no (head-var masking) |
| `EachChildThreeConjunctsTest` | 3 conjuncts: `bag_of` → `length` → `member`; negative case for `julien` (1 child ⇒ `length(L,3)` fails) | **yes** |

In `tests/LogsharpTests/PrologBuiltinsTests.cs` (program extended with
`mapped_and_found()`, `mapped_no_aa()`, `rules_then_length(L,N)`,
`length_one_shot(X)`):

| Test | What it pins |
|------|--------------|
| `MapListAsFirstConjunctTest` | `member` consumes a body-local list from `map_list` |
| `MapListWithNotTest` | `not/1` discriminating: `not(member(aa,L))` must *fail* the rule because `aa` **is** in `L` — if `L` were lost, the rule would wrongly succeed |
| `LengthAfterMapListTest` | `length` consumes `map_list`'s output; `L` and `N` flow through the head |
| `BuiltInNotReExecutedAfterItsContinuationTest` | `length` followed by `single/1`: exactly 2 answers (guards against the double-execution duplication) |
| `PreludeDistinctElementsFastTest` | prelude rule `distinct_elements_fast` — a body-local `Sorted` flowing between `sort/2` and `length/2` |

Full suite: **46/46 green**. Verified against the pre-fix interpreter
(commit `995668d`): `ChildOfTest` and `EachChildThreeConjunctsTest` fail, the
rest pass (head-variable masking — see above).
