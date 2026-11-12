# LogSharp Toplevel — interactive and CLI usage

`Toplevel` is the command-line front-end of LogSharp (project
`src/Toplevel`). It loads one or more LogSharp program files and lets you ask
questions against them.

It has two input modes:

| Mode | When | Solutions |
| --- | --- | --- |
| **Interactive** | stdin is a real terminal | one solution at a time, each followed by an `encore / stop ?` prompt |
| **Non-interactive (CLI / piped)** | stdin is a pipe or a redirected file | **all** solutions are printed, with no validation |

The mode is detected automatically with `Console.IsInputRedirected`; there is
nothing to configure.

**Table of contents**

- [Running Toplevel](#running-toplevel)
- [Providing programs](#providing-programs)
- [Options](#options)
- [Interactive mode](#interactive-mode)
- [Non-interactive (piped) mode](#non-interactive-piped-mode)
- [REPL commands](#repl-commands)
- [Output format](#output-format)
- [Full examples](#full-examples)
- [Notes and current limitations](#notes-and-current-limitations)

## Running Toplevel

From the repository root, run the project and pass the program file(s) after
`--`:

```bash
dotnet run --project src/Toplevel -- src/Toplevel/test.pro
```

Or run an already built binary directly:

```bash
src/Toplevel/bin/Debug/net10.0/Toplevel src/Toplevel/test.pro
```

With no program file, Toplevel starts empty and you can add rules from the
prompt (or load them with the [`load` command](#repl-commands)).

## Providing programs

- The **first positional argument** is the main program file. It is parsed as a
  whole `program` and handed to the interpreter.
- Any **additional positional arguments** are extra program files, loaded and
  appended on top of the main one:

  ```bash
  Toplevel base.pro rules.pro facts.pro
  ```

- At runtime you can load more files with the `load` command (see
  [REPL commands](#repl-commands)).

The LogSharp prelude (`prelude.pro`) and all built-in predicates are always
loaded automatically.

## Options

| Option | Meaning |
| --- | --- |
| `-v <file>` / `--verbose <file>` / `--verbosity <file>` | Enable verbose tracing and also append the trace to `<file>`. |

Example (the option is placed **after** the program file):

```bash
Toplevel test.pro -v /tmp/trace.log
```

You can also toggle tracing from the prompt at any time with the
[`trace` / `quiet` commands](#repl-commands).

## Interactive mode

Launch Toplevel from a terminal (stdin is a TTY). It prints a `?. ` prompt:

- A line ending with **`.`** is a fact or a rule and is appended to the program.
- A line ending with **`?`** is a question and is resolved.
- `quit` or `exit` (or end-of-input) leaves the REPL.

When a question has several solutions, the engine does **not** compute them all
up front. It prints one solution, then asks:

```text
encore / stop ?
```

- `stop` stops the search. Because the search is lazy, the remaining solutions
  are **never computed**.
- Anything else (`encore`, or just `Enter`) asks for the next solution, which is
  then computed on demand.
- When the query has no solution, the single `don't know.` result is printed
  **without** a prompt.

### Example session

```text
$ Toplevel test.pro
?. t(X)?
yes.
[ X = z ]
encore / stop ? encore
yes.
[ X = y ]
encore / stop ? stop
?. grade(alice, Subject, Grade)?
yes.
[ Subject = science , Grade = 90 ]
encore / stop ? stop
?. quit
```

## Non-interactive (piped) mode

When stdin is not a terminal — a pipe or a redirected file — Toplevel switches to
non-interactive mode:

- it reads every input line (`Console.ReadLine`), with no `?. ` prompt;
- for a question it prints **all** solutions, one after another, with **no**
  `encore / stop` validation;
- `quit` / `exit` and end-of-file stop the loop.

This is the mode to use from scripts or to feed a question into the CLI:

```bash
# a single question, all answers printed
echo 't(X)?' | Toplevel test.pro
```

```text
yes.
[ X = z ]
yes.
[ X = y ]
yes.
[ X = x ]
```

```bash
# a question stored in a file
Toplevel test.pro < question.txt
```

```bash
# feed the program itself, then the question, on stdin
printf 't(x).\nt(y).\nt(X)?\n' | Toplevel
```

Each input line is parsed on its own, so piped rules/facts must be on a single
line (a multi-line rule cannot be piped).

## REPL commands

Special questions (recognised by name) drive the REPL instead of running a
query:

| Command | Effect |
| --- | --- |
| `load(Name)?` | Load a program file. `Name` is an atom or a quoted string; the `.pl` / `.pro` extension is optional. `load(test)?` loads `test.pro`. |
| `trace(Name)?` | Enable verbose tracing and write it to `Name` (atom or quoted string). |
| `quiet(Name)?` | Disable verbose tracing (`Name` is ignored). |
| `dump(rules)?` | Print the rules of the current program. `dump(all)?` also lists the built-ins. |
| `dump(builtin)?` | Print the registered built-in predicates. |

Examples:

```text
?. load(test)?
yes.
?. dump(rules)?
t(x).
t(y).
...
?. dump(builtin)?
number/1
assert/1
is/2
...
```

## Output format

A successful query prints:

```text
yes.
[ X = z ]
```

with the variable bindings listed on a single line inside the brackets,
separated by ` , ` (`[ Subject = science , Grade = 90 ]`). A query with no
solution prints:

```text
don't know.
```

## Full examples

**Interactive, exploring solutions one by one:**

```text
$ Toplevel test.pro
?. grade(alice, Subject, Grade)?
yes.
[ Subject = science , Grade = 90 ]
encore / stop ? encore
yes.
[ Subject = math , Grade = 85 ]
encore / stop ? stop
?. quit
```

**Piped question, every answer:**

```bash
echo 'grade(alice, Subject, Grade)?' | Toplevel test.pro
```

```text
yes.
[ Subject = science , Grade = 90 ]
yes.
[ Subject = math , Grade = 85 ]
```

**Piped program + question (no program file argument):**

```bash
printf 'parent(tom, bob).\nparent(bob, ann).\nparent(X, Y)?\n' | Toplevel
```

```text
yes.
[ X = bob , Y = ann ]
yes.
[ X = tom , Y = bob ]
```

**Using `dotnet run`:**

```bash
echo 't(X)?' | dotnet run --project src/Toplevel -- src/Toplevel/test.pro
```

## Notes and current limitations

- **REPL commands need an argument.** Parameterless forms such as `quiet?`,
  `trace?` or `dump?` are parsed as ordinary queries today; pass a value
  (`quiet(x)?`, `trace(out.log)?`, `dump(rules)?`) to invoke them.
- **Option placement.** `-v` must come *after* the program file(s). Placing it
  first makes Toplevel try to read `-v` as a program file.
- **One line per input.** In non-interactive mode each line is parsed
  independently, so a rule spanning several lines cannot be piped. Put the
  program in a file and pass it as an argument instead.
- **Lazy search.** In interactive mode, remaining solutions are computed only
  when you answer `encore`; typing `stop` abandons the search immediately.
