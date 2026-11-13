# LogSharp prelude library

This document lists every predicate provided by the prelude library, loaded
automatically by every `Interpreter` (see `src/LogSharp/prelude/prelude.pro`,
parsed and appended at construction by `PreludeLoader`). Those predicates are
the ones that are useful enough not to rewrite them in every program: they are
plain LogSharp rules, written in LogSharp itself.

For the built-in predicates implemented in C#, see [builtins.md](builtins.md).

**Table of contents**

- [Grammar reminders](#grammar-reminders)
- [Original prelude predicates](#original-prelude-predicates)
- [List building and slicing](#list-building-and-slicing)
- [List access and aggregation](#list-access-and-aggregation)
- [Membership and enumeration](#membership-and-enumeration)
- [Filtering](#filtering)
- [Folding](#folding)

## Grammar reminders

The prelude follows the LogSharp dialect, which differs slightly from standard
Prolog (the full table is in [builtins.md](builtins.md#grammar-reminders)):

- goals in a rule body are separated by **spaces**, not commas;
- arithmetic goes through `is/2` (`Y is X + 1`);
- bare comparisons (`X > 0`) work as goals, but **both sides must be bound**,
  otherwise evaluation fails;
- there is no `=` unification goal: results flow through the rule arguments,
  and `is/2` (whose left side must be a variable) binds results.

## Original prelude predicates

### `my_maplist/2` and `my_maplist/3`

```
my_maplist(Goal, List)
my_maplist(Goal, List1, List2)
```

Pure-Prolog versions of maplist, using `call/2` (resp. `call/3`) and
recursion. Prefer the `maplist/2` built-in for constraint programs (it merges
the bindings of each element call).

### `distinct_elements_fast/1`

```
distinct_elements_fast(List)
```

Succeeds when `List` contains no duplicates, by comparing the length of
`sort(List, Sorted)` with `length(List, Len)`.

### `len/2`

```
len(X, Y)
```

Alias for `length/2`.

## List building and slicing

### `append/3`

```
append(Prefix, Suffix, Whole)
```

Succeeds when `Whole` is the concatenation of `Prefix` and `Suffix`.
(Note: the built-in `append/2` flattens a list of lists — a different thing.)

Useful in every direction:

- `append([1,2],[3,4],W)` — joins two lists, `W = [1,2,3,4]`;
- `append(P,[3,4],[1,2,3,4])` — finds the unique split, `P = [1,2]`;
- with `Prefix` free and `Whole` bound, enumerates every prefix (see
  `prefix/2`).

### `prefix/2`

```
prefix(Prefix, Whole)
```

Succeeds when `Prefix` is an initial segment of `Whole`. With `Prefix` free,
enumerates every prefix of `Whole`, longest first (`[1,2,3]`, `[1,2]`, `[1]`,
`[]`).

### `suffix/2`

```
suffix(Suffix, Whole)
```

Succeeds when `Suffix` is a final segment of `Whole`. With `Suffix` free,
enumerates every suffix of `Whole`, shortest first (`[]`, `[3]`, `[2,3]`,
`[1,2,3]`).

### `sublist/2`

```
sublist(Sub, Whole)
```

Succeeds when `Sub` appears as a contiguous slice of `Whole`. Non-contiguous
subsequences (`sublist([1,3],[1,2,3,4])`) fail.

## List access and aggregation

### `last/2`

```
last(List, Element)
```

`Element` is the last element of `List`. Fails on `[]`.

### `nth0/3`

```
nth0(Index, List, Element)
```

0-based element access, working in both directions:

- `Index` bound: checks/binds the element at that position
  (`nth0(1,[a,b,c],E)` gives `E = b`);
- `Index` free: enumerates every `(Index, Element)` pair of the bound list.

### `nth1/3`

```
nth1(Index, List, Element)
```

Same as `nth0/3` but 1-based (element numbering starts at 1). Defined as
`nth0(ZeroBased, List, Element) Index is ZeroBased + 1`.

### `reverse/2`

```
reverse(List, Reversed)
```

`Reversed` holds the elements of `List` in reverse order (accumulator style,
so reversing stays linear). Can also check an already reversed list.

### `sum_list/2`

```
sum_list(List, Sum)
```

`Sum` is the sum of the (numeric) elements of `List`; `sum_list([], 0)`.

### `max_list/2` and `min_list/2`

```
max_list(List, Max)
min_list(List, Min)
```

`Max` / `Min` is the greatest / smallest element of a non-empty numeric
`List`.

## Membership and enumeration

### `memberchk/2`

```
memberchk(Element, List)
```

Succeeds **exactly once** if `Element` occurs in `List`, and fails otherwise.
Unlike `member/2`, it leaves no alternative solutions behind — useful to test
membership without enumerating duplicates. Implemented with the double
negation trick `not(not(member(Element, List)))` instead of a cut.

### `between/3`

```
between(Low, High, X)
```

- `X` free: one solution per integer in the inclusive range `[Low..High]`.
- `X` bound: succeeds once when `Low <= X <= High` (no duplicates).

`Low` and `High` must be bound integers (guarded with `number/1`, since a
comparison goal with an unbound side raises an evaluation error).

### `range/3`

```
range(Low, High, List)
```

Builds the inclusive list `[Low, Low+1, ..., High]`; the list is empty when
`Low > High`.

### `select/3`

```
select(Element, List, Rest)
```

Removes one occurrence of `Element` from `List`, giving `Rest`. With
`Element` free, picks each element of the bound `List` in turn (one solution
per element position). Fails when `Element` does not occur.

### `permutation/2`

```
permutation(List, Perm)
```

`Perm` is a permutation of `List`. With `Perm` free, enumerates every
permutation (via `select/3`); with `Perm` bound, checks the pair.

## Filtering

Both predicates take a unary goal applied to each element through `call/2`.
The **goal should succeed at most once per element**, otherwise duplicated
elements appear in the result.

### `include/3`

```
include(Goal, List, Included)
```

`Included` keeps the elements of `List` for which `Goal` succeeds. Example:

```
big(X) :- number(X) X > 10.
include(big,[5,11,12,3],I)   % -> I = [11,12]
```

### `exclude/3`

```
exclude(Goal, List, Excluded)
```

`Excluded` drops the elements of `List` for which `Goal` succeeds (keeps the
others). Example: `exclude(big,[5,11,12,3],E)` gives `E = [5,3]`.

## Folding

### `foldl/4`

```
foldl(Goal, List, AccIn, AccOut)
```

Reduces `List` left to right with an accumulator: for each element `Element`,
`Goal/3` is invoked through `call(Goal, Element, Acc, NextAcc)` and the final
accumulator is unified with `AccOut`. With an empty list, `AccOut` is `AccIn`
unchanged (`foldl(_, [], Acc, Acc)`).

Like `include/3` and `exclude/3`, `Goal` must be a predicate name (an atom),
and the goal should succeed exactly once per element: a failing goal makes the
whole fold fail, while several successes would enumerate alternative folds.

Examples (with `add(X, Y, Z) :- Z is X + Y.`):

```
foldl(add,[1,2,3],0,S)   % -> S = 6
foldl(add,[],5,S)        % -> S = 5
```

## Adding a prelude predicate

The prelude is an ordinary `.pro` program embedded as a resource of the
`LogSharp` assembly (`prelude.pro` in `src/LogSharp/prelude/`). To add a
predicate:

1. append it to `prelude.pro`, following the dialect (space-separated goals,
   `is/2` for arithmetic, `number/1` guards before comparison goals);
2. document it in this file;
3. add unit tests in `tests/LogsharpTests/PreludeTests.cs` — the prelude is
   loaded by every `BaseTest` interpreter, so no extra setup is needed.
