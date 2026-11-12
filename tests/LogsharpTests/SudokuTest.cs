using System;
using System.IO;
using System.Linq;
using logSharp.interpretation.unification;
using logSharp.model;
using NFluent;
using Xunit;

namespace LogsharpTests;

public class SudokuTest : BaseTest
{
    private const int Size = 9;

    public SudokuTest()
        : base(ReadSudokuProgram())
    {
    }

    private static string ReadSudokuProgram()
    {
        var assembly = typeof(SudokuTest).Assembly;
        using var stream = assembly.GetManifestResourceStream("LogsharpTests.programs.sudoku.pro");
        Check.That(stream).IsNotNull();
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    [Fact]
    public void CompleteValidGridIsAccepted()
    {
        var answers = GetAnswers($"sudoku({GridLiteral(WikipediaSolution())})?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0].IsOk).IsTrue();
    }

    [Fact]
    public void GridWithDuplicateInRowIsRejected()
    {
        var grid = WikipediaSolution();
        grid[0, 0] = grid[0, 2]; // duplicate 4 in the first row
        var answers = GetAnswers($"sudoku({GridLiteral(grid)})?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0].IsOk).IsFalse();
    }

    [Fact]
    public void GridWithDuplicateInBlockIsRejected()
    {
        var grid = WikipediaSolution();
        grid[1, 1] = grid[0, 0]; // duplicate 5 inside the top-left block
        var answers = GetAnswers($"sudoku({GridLiteral(grid)})?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0].IsOk).IsFalse();
    }

    [Fact]
    public void GridWithDuplicateInColumnIsRejected()
    {
        var grid = WikipediaSolution();
        grid[8, 0] = grid[0, 0]; // duplicate 5 in the first column
        var answers = GetAnswers($"sudoku({GridLiteral(grid)})?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0].IsOk).IsFalse();
    }

    [Fact]
    public void SolvesTheWikipediaPuzzle()
    {
        // the classic Wikipedia puzzle : every 0 is a free cell, replaced by a
        // fresh variable so that label/1 can assign it
        var answers = GetAnswers($"sudoku({PuzzleLiteral()})?");
        Check.That(answers).CountIs(1);
        Check.That(answers[0].IsOk).IsTrue();

        // every free cell must have been instantiated to the known solution
        var solution = WikipediaSolution();
        var puzzle = WikipediaPuzzle();
        var next = 1;
        for (var row = 0; row < Size; row++)
        {
            for (var column = 0; column < Size; column++)
            {
                if (puzzle[row, column] == 0)
                {
                    var binding = answers[0].GetBinding($"V{next++}");
                    Check.WithCustomMessage($"V{next - 1} [{row},{column}]")
                        .That(binding is Int value && value.Value == solution[row, column]).IsTrue();
                }
            }
        }
    }

    #region grid helpers

    // the unique solution of the classic Wikipedia puzzle
    private static int[,] WikipediaSolution()
    {
        return new[,]
        {
            {5, 3, 4, 6, 7, 8, 9, 1, 2},
            {6, 7, 2, 1, 9, 5, 3, 4, 8},
            {1, 9, 8, 3, 4, 2, 5, 6, 7},
            {8, 5, 9, 7, 6, 1, 4, 2, 3},
            {4, 2, 6, 8, 5, 3, 7, 9, 1},
            {7, 1, 3, 9, 2, 4, 8, 5, 6},
            {9, 6, 1, 5, 3, 7, 2, 8, 4},
            {2, 8, 7, 4, 1, 9, 6, 3, 5},
            {3, 4, 5, 2, 8, 6, 1, 7, 9},
        };
    }

    // the classic Wikipedia puzzle : 0 is a free cell
    private static int[,] WikipediaPuzzle()
    {
        return new[,]
        {
            {5, 3, 0, 0, 7, 0, 0, 0, 0},
            {6, 0, 0, 1, 9, 5, 0, 0, 0},
            {0, 9, 8, 0, 0, 0, 0, 6, 0},
            {8, 0, 0, 0, 6, 0, 0, 0, 3},
            {4, 0, 0, 8, 0, 3, 0, 0, 1},
            {7, 0, 0, 0, 2, 0, 0, 0, 6},
            {0, 6, 0, 0, 0, 0, 2, 8, 0},
            {0, 0, 0, 4, 1, 9, 0, 0, 5},
            {0, 0, 0, 0, 8, 0, 0, 7, 9},
        };
    }

    private static string GridLiteral(int[,] grid)
    {
        var rows = new string[Size];
        for (var row = 0; row < Size; row++)
        {
            var cells = new string[Size];
            for (var column = 0; column < Size; column++)
            {
                cells[column] = grid[row, column].ToString();
            }

            rows[row] = "[" + string.Join(",", cells) + "]";
        }

        return "[" + string.Join(",", rows) + "]";
    }

    // builds the puzzle query : every free cell becomes a fresh variable V1, V2, ...
    private static string PuzzleLiteral()
    {
        var puzzle = WikipediaPuzzle();
        var next = 1;
        var rows = new string[Size];
        for (var row = 0; row < Size; row++)
        {
            var cells = new string[Size];
            for (var column = 0; column < Size; column++)
            {
                cells[column] = puzzle[row, column] == 0 ? $"V{next++}" : puzzle[row, column].ToString();
            }

            rows[row] = "[" + string.Join(",", cells) + "]";
        }

        return "[" + string.Join(",", rows) + "]";
    }

    #endregion
}

public class FourByFourSudokuTest : BaseTest
{
    private const int Size = 4;

    private const string Program = @"
% 4x4 sudoku variant : same clpfd-like structure as the 9x9 sudoku
sudoku4(Rows) :-
    length(Rows, 4)
    maplist(same_length(Rows), Rows)
    append(Rows, Vs)
    Vs ins 1 ... 4    
    maplist(all_distinct, Rows)
    transpose(Rows, Columns)
    maplist(all_distinct, Columns)
    as_grid4(Rows, R1, R2, R3, R4)
    blocks4(R1, R2, R3, R4)
    label(Vs).

as_grid4([R1,R2,R3,R4], R1, R2, R3, R4).

blocks4([], [], [], []).
blocks4([A,B|As],[C,D|Cs],[E,F|Es],[G,H|Gs]) :-
    all_distinct([A,B,C,D])
    all_distinct([E,F,G,H])
    blocks4(As, Cs, Es, Gs).
";

    public FourByFourSudokuTest() : base(Program)
    {
    }

    [Fact]
    public void SolvesThePuzzle()
    {
        var answers = GetAnswers("sudoku4(Rows)?");
        Check.That(answers.Count).IsStrictlyPositive();
        Check.That(answers[0].IsOk).IsTrue();

        var grid = ToGrid(answers[0]);
        Check.WithCustomMessage("the first solution must be a valid 4x4 sudoku")
            .That(IsValid(grid)).IsTrue();
    }

    [Fact]
    public void EnumeratesEverySolution()
    {
        // there are exactly 288 solutions of the 4x4 sudoku
        var answers = GetAnswers("sudoku4(Rows)?");
        Check.That(answers).CountIs(288);
        Check.That(answers.All(answer => answer.IsOk)).IsTrue();
    }

    private static int[,] ToGrid(Unification answer)
    {
        var rows = (answer.GetBinding("Rows") as Term).ToValuableList();
        var grid = new int[Size, Size];
        for (var row = 0; row < Size; row++)
        {
            var current = rows[row];
            while (current is Variable variable && answer.HasBinding(variable.Name))
            {
                current = answer.GetBinding(variable.Name);
            }

            var cells = (current as Term).ToValuableList();
            for (var column = 0; column < Size; column++)
            {
                var cell = cells[column];
                while (cell is Variable variable && answer.HasBinding(variable.Name))
                {
                    cell = answer.GetBinding(variable.Name);
                }

                grid[row, column] = (cell as Int).Value;
            }
        }

        return grid;
    }

    private static bool IsValid(int[,] grid)
    {
        var box = 2;
        for (var index = 0; index < Size; index++)
        {
            var rowSeen = new bool[Size + 1];
            var columnSeen = new bool[Size + 1];
            for (var other = 0; other < Size; other++)
            {
                if (rowSeen[grid[index, other]] || columnSeen[grid[other, index]])
                {
                    return false;
                }

                rowSeen[grid[index, other]] = true;
                columnSeen[grid[other, index]] = true;
            }
        }

        for (var boxRow = 0; boxRow < Size; boxRow += box)
        {
            for (var boxColumn = 0; boxColumn < Size; boxColumn += box)
            {
                var seen = new bool[Size + 1];
                for (var row = boxRow; row < boxRow + box; row++)
                {
                    for (var column = boxColumn; column < boxColumn + box; column++)
                    {
                        if (seen[grid[row, column]])
                        {
                            return false;
                        }

                        seen[grid[row, column]] = true;
                    }
                }
            }
        }

        return true;
    }
}
