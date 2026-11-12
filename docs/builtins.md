# LogSharp built-in predicates

This document lists every built-in predicate available to LogSharp programs,
grouped by category, with their semantics and binding modes.

**Table of contents**

- [How built-ins are registered](#how-built-ins-are-registered)
- [Grammar reminders](#grammar-reminders)
- [Control and meta-call](#control-and-meta-call)
- [Type tests](#type-tests)
- [Arithmetic and expressions](#arithmetic-and-expressions)
- [Lists](#lists)
- [All-solutions predicates](#all-solutions-predicates)
- [Finite domains (clpfd-like)](#finite-domains-clpfd-like)
- [Definite clause grammars (DCG)](#definite-clause-grammars-dcg)
- [Prolog prelude (written in LogSharp itself)](#prolog-prelude-written-in-logsharp-itself)

## How built-ins are registered

Built-ins are plain .NET methods decorated with a `[BuiltIn("name/arity")]`
attribute, gathered in classes decorated with `[BuiltIns]`. At interpreter
construction, `BuiltinsLoader` scans the loaded assemblies for those classes and
registers every method in the interpreter's `BuiltIns` dictionary (see
`src/LogSharp/builtins/BuiltinsLoader.cs`). Methods decorated with
`[BuiltInBuilder]` may register whole families of built-ins (used by
`call/2..9` and `not/2..9`).

A built-in is called when a goal in a rule body (or a query) matches its
`name/arity` specification: built-ins take precedence over user-defined rules
(see `Interpreter.Search` — the interpreter first checks its `BuiltIns`
dictionary and only falls back to the program's rules otherwise).

A built-in returns a list of unifications: **one per solution**, so a built-in
can be multi-solution and the surrounding query will backtrack through every
answer. An empty result (or a single failed unification) means failure.

## Grammar reminders

The LogSharp dialect is close to Prolog but not identical:

| Topic | LogSharp | Standard Prolog |
| --- | --- | --- |
| Goals in a rule body | separated by **spaces** | separated by commas |
| Query | ends with `?` (e.g. `parent(X, bob)?`) | `?- parent(X, bob).` |
| Cut | `!` (internal name `cut/0`) | `!` |
| Equality comparison | `==` | `==` |
| Not unifiable | `\=` | `\=` |
| Inequality comparison | `!=` | `\==` |
| Arithmetic evaluation | `X is Expr` | `X is Expr` |
| Finite-domain range | `Vars ins Min...Max` | `Vars ins Min..Max` |
| Directives | not supported (`:- ... .`) | supported |

## Control and meta-call

### `call/1` — `call/9`

```
call(Goal)
call(Goal, Arg1)      ... call(Goal, Arg1, ..., Arg8)
```

Calls `Goal` as a goal. With extra arguments, the arguments are appended to the
goal: `call(foo, X, Y)` behaves like `foo(X, Y)`. The first argument may be an
atom or a partially applied term. The call fails when the target is neither a
user rule nor a built-in of the matching arity.

`call/2` to `call/9` are generated at load time by a `[BuiltInBuilder]`.

### `not/1` — `not/9`

```
not(Goal)
not(Goal, Arg1)       ... not(Goal, Arg1, ..., Arg8)
```

Negation as failure: succeeds (without producing bindings) when `Goal` has no
solution, and fails when `Goal` has at least one solution. Input bindings
already present in the current unification are visible to `Goal`.

### `!` (`cut/0`)

```
!
```

Commits to the current choice: prunes the remaining alternative choices of the
surrounding goal (see `Interpreter.Cut`). Written as `!` in programs; `cut/0`
is its internal name.

Typical use, together with `fail/0`, to enumerate every solution of a goal:

```
no(T) :- call(T) ! fail().
no(T).
```

### `fail/0`

```
fail
```

Always fails; forces backtracking into the remaining choices.

### `assert/1`

```
assert(Expression)
```

Evaluates an expression (comparison or arithmetic) in the current unification
and succeeds when it is true. This is the internal form used for bare
expression goals: writing `3 == 3` in a rule body is parsed as
`assert(3 == 3)`.

## Type tests

### `atom/1`

```
atom(X)
```

Succeeds when `X` (after resolving its binding) is an atom.

### `number/1`

```
number(X)
```

Succeeds when `X` is an integer or a double.

## Arithmetic and expressions

### `is/2`

```
X is Expression
```

Evaluates the arithmetic/expression tree and binds the free variable `X` to the
result (`Int`, `Double` or `String`). When both sides are already evaluable
expressions, the goal succeeds when both evaluate to the same value.

### Infix expression operators

Available inside expressions (parsed by the expression grammar, usable both in
`is/2` and as standalone comparison goals):

| Operator | Meaning | Notes |
| --- | --- | --- |
| `+` `-` `*` `/` | arithmetic | right-associative `+`/`*`, left-associative `-`/`/`; `+` on strings concatenates |
| `==` | equal | comparison of the evaluated values |
| `!=` | different | |
| `<` `>` `<=` `>=` | ordering | |
| `\=` | not unifiable | comparison based |

Comparison goals (e.g. `X > 3`) succeed when the relation holds between the two
evaluated sides.

## Lists

All list built-ins accept proper lists written `[a, b, c]` and pipe lists
`[Head | Tail]` / `[X1, X2 | Tail]`.

### `member/2`

```
member(Needle, Haystack)
```

- `Needle` bound: succeeds when the value occurs in the bound list `Haystack`.
- `Needle` free: one solution per element of `Haystack`, binding `Needle` to
  each element in turn.

`Haystack` must be a bound list.

### `length/2`

```
length(List, Length)
```

Two binding directions:

- `List` bound: binds `Length` to the element count, or checks it when already
  bound.
- `Length` bound (and `List` a free variable): generates a list of `Length`
  fresh variables and binds it to `List` — this is how
  `length(Rows, 9)` builds an empty 9-row grid.

### `same_length/2`

```
same_length(List1, List2)
```

- both bound: succeeds when the two lists have the same number of elements.
- one bound, the other free: generates a list of fresh variables of the right
  length and binds it to the free side.

Used with `maplist/2` to build a grid with a fixed row length, e.g.
`maplist(same_length(Rows), Rows)`.

### `append/2`

```
append(ListOfLists, FlatList)
```

Flattens a list of lists by exactly one level. The first argument must be
bound; the second argument is bound to the flattened list (or checked when
already bound). Elements that are not lists are kept as-is.

Example: `append([[1,2],[3],[4,5,6]], Vs)` gives `Vs = [1, 2, 3, 4, 5, 6]`.

### `sort/2`

```
sort(List, Sorted)
```

- `List` bound: binds `Sorted` to the sorted, duplicate-free list (or checks
  it when already bound).
- `Sorted` bound and `List` free: succeeds when the bound value is already
  sorted and duplicate-free, and binds `List` to it.

Ordering: numbers first (ascending), then atoms/strings by their dumped
representation.

### `map_list/3`

```
map_list(List, Query, Output)
```

Applies the binary goal `Query(In, Out)` to every element of the bound list
`List` and binds `Output` to the list of the resulting `Out` values. Fails as
soon as `Query` fails for one element. The two arguments of `Query` must be
variables.

## All-solutions predicates

### `find_all/3`

```
find_all(Template, Goal, List)
```

Binds `List` to the list of every `Template` instantiation for which `Goal`
succeeds. `Template` must be a variable; `Goal` a term. Fails when `Goal` has
no solution (no empty list is produced).

Example (from the test suite):

```
find_all(Score, grade(Student, Subject, Score), Scores)?
```

### `bag_of/3`

```
bag_of(Template, Goal, Bag)
```

Like `find_all/3` but **grouped**: the free variables of `Goal` that do not
occur in `Template` (the "additional" variables) act as group keys. One
solution is produced per distinct combination of additional-variable values,
and those variables are bound in the answer; `Bag` holds the `Template` values
collected for that group. `Bag` may also be a bound list, in which case it is
checked against the group's bag.

Example: `bag_of(Score, grade(Student, Subject, Score), Scores)?` produces one
answer per `Student` (and per `Subject`, for the variables left free).

### `set_of/3`

```
set_of(Template, Goal, Set)
```

Same as `bag_of/3` with duplicates removed from every group bag (the order of
first appearance is kept).

## Finite domains (clpfd-like)

These built-ins emulate the constraint part of `library(clpfd)` for integer
variables. Constraints are recorded in the interpreter's constraint store when
a goal is called with free variables, and enforced later by `label/1`.

Constraint variables are identified by name, so a variable occurring in a
`transpose/2` result carries the same constraints as in the original matrix.

### `all_distinct/1`

```
all_distinct(List)
```

Every element of `List` must be pairwise different.

- Bound elements are checked immediately: two equal bound values fail the
  goal.
- Free variables are recorded as a distinct-group constraint enforced by
  `label/1`.
- `_` elements are ignored.
- A variable appearing twice in the list fails the goal immediately.

### `ins_range/3`

```
Vars ins Min...Max
ins_range(Vars, Min, Max)
```

Constrains every variable of `Vars` to the inclusive integer range
`[Min, Max]`. In programs it is written with the infix **spread operator**
syntax:

```
Vs ins 1...9
```

The parser translates that sugar into the `ins_range/3` built-in term, which
can also be called in its functional form (`ins_range(Vs, 1, 9)`) since
`ins_range` is a plain functor — only `ins` itself is a reserved word of the
grammar.

**The bounds are not restricted to integer literals** — each bound is an
expression that is evaluated in the current unification, so all of the
following work:

```
Vs ins 1...9          % integer literals
Vs ins Min...Max      % variables, previously bound e.g. with is/2
Vs ins 1...2 + 1      % arithmetic expressions (here Max = 3)
```

A bound must evaluate to an integral value: `2.5` is rejected, while `3.0` is
accepted as `3`.

- Free variables receive the domain (an `ins_range/3` declaration overwrites a
  previously declared domain for the same variable).
- Already bound elements are range-checked immediately.
- The first argument must be a list of variables/ints (a bare variable fails
  the goal).

### `label/1`

```
label(Vars)
```

Assigns a value from the variable domain to every free variable of `Vars`,
backtracking over the recorded constraints (`all_distinct/1` groups and
`ins_range/3` domains). **One unification is returned per complete consistent
assignment**, so the surrounding query enumerates every solution.

- Variables without an `ins_range/3` declaration default to the domain `1..9`.
- Already bound variables are not labeled; they are respected as fixed values
  by the distinct-group checks.
- A fully bound list trivially succeeds (constraints were already checked at
  declaration time).
- Note: solutions are computed eagerly (all of them, as a list), so the size
  of the search space should stay reasonable.

Typical usage, as in the sudoku example:

```
sudoku(Rows) :-
    length(Rows, 9)
    maplist(same_length(Rows), Rows)
    append(Rows, Vs)
    Vs ins 1...9
    maplist(all_distinct, Rows)
    transpose(Rows, Columns)
    maplist(all_distinct, Columns)
    blocks(Rows)
    label(Vs).
```

### `transpose/2`

```
transpose(Matrix, Transposed)
```

Transposes a rectangular list of lists. Works in three modes:

- `Matrix` bound, `Transposed` free: binds the transposed matrix.
- both bound: checks the pair.
- `Matrix` free, `Transposed` bound: binds `Matrix` to the transpose of the
  transpose (i.e. to `Transposed` itself).

The cell values (variables or integers) are **shared** between the two
matrices, so constraints recorded through one of them apply to the other —
that is how column constraints are enforced in the sudoku program.

### `maplist/2`

```
maplist(Goal, List)
```

Applies the unary goal `Goal` to every element of the bound list `List`. `Goal`
may be an atom or a partially applied term (e.g. `maplist(same_length(Rows),
Rows)` calls `same_length(Rows, Element)` for each element). The goal is
searched for its first solution per element; bindings made by the goal are
merged into the surrounding unification. Fails as soon as one element fails.

## Definite clause grammars (DCG)

Grammar rules are written with the `-->` arrow and are translated by
`DcgTranslator` into ordinary predicates taking the input and output token
lists as two extra arguments.

### `phrase/2`

```
phrase(GrammarRule, Input)
```

Calls the DCG rule `GrammarRule` against the token list `Input`:
`phrase(sentence, Tokens)` invokes `sentence(Tokens, [])`.

## Prolog prelude (written in LogSharp itself)

`src/LogSharp/prelude/prelude.pro` is parsed and appended to every interpreter
at construction (`PreludeLoader`). It provides:

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

## Extending the built-in set

To add a new built-in:

1. Create (or extend) a class decorated with `[BuiltIns]`.
2. Add a static method with signature
   `List<Unification> Method(Interpreter interpreter, Goal goal, List<Valuable> parameters)`
   decorated with `[BuiltIn("name/arity")]`.
3. `parameters` arrive resolved (variables replaced by their bound values when
   possible, unbound variables preserved); return one `Unification` per
   solution, using `Unification.Success()` / `Unification.Failed()` and
   `ListHelper.BuildList(...)` to build the result list.

For variable families (`call/2..9`, `not/2..9`), use a method decorated with
`[BuiltInBuilder]` receiving the interpreter and calling
`interpreter.AddBuiltIn(spec, builtin)` for each variant.
