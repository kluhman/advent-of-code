using AdventOfCode.Core;
using AdventOfCode.Core.Extensions;

namespace AdventOfCode2023;

public class Day13PointOfIncidence : IChallenge
{
    public int ChallengeId => 13;

    public object SolvePart1(string input) => GetGrids(input).Sum(FindGridReflection);

    public object SolvePart2(string input) => GetGrids(input).Sum(FindGridSmudges);

    private static long FindGridReflection(string[] rows)
    {
        var score = 0L;
        score += Enumerable
            .Range(1, rows[0].Length - 1)
            .Where(point => rows.All(row => HasReflection(row, point)))
            .Sum();

        var columns = GetColumns(rows);
        score += 100L * Enumerable
            .Range(1, columns[0].Length - 1)
            .Where(point => columns.All(column => HasReflection(column, point)))
            .Sum();

        return score;
    }

    private static string[] GetColumns(string[] grid)
    {
        var width = grid[0].Length;
        var columns = new string[width];
        for (var x = 0; x < width; x++)
        {
            columns[x] = new string(grid.Select(line => line[x]).ToArray());
        }

        return columns;
    }

    private static bool HasReflection(string value, int position)
    {
        for (var offset = 0; offset < position; offset++)
        {
            var leftIndex = position - offset - 1;
            var rightIndex = position + offset;

            if (leftIndex < 0 || rightIndex >= value.Length)
            {
                break;
            }

            if (value[leftIndex] != value[rightIndex])
            {
                return false;
            }
        }

        return true;
    }

    private static long FindGridSmudges(string[] rows)
    {
        var score = 0L;
        score += Enumerable
            .Range(1, rows[0].Length - 1)
            .Where(point => CheckForSmudgeReflection(rows, point))
            .Sum();

        var columns = GetColumns(rows);
        score += 100L * Enumerable
            .Range(1, columns[0].Length - 1)
            .Where(point => CheckForSmudgeReflection(columns, point))
            .Sum();

        return score;
    }

    private static bool CheckForSmudgeReflection(string[] rows, int point) => rows
        .Select((row, index) => new
        {
            Index = index,
            CouldBeSmudged = CouldHaveSmudge(row, point)
        })
        .Where(x => x.CouldBeSmudged)
        .Any(smudgedRow =>
        {
            var previousRows = rows[..smudgedRow.Index];
            var upcomingRows = rows[(smudgedRow.Index + 1)..];

            return previousRows.Concat(upcomingRows).All(row => HasReflection(row, point));
        });

    private static bool CouldHaveSmudge(string value, int position)
    {
        var potentialSmudge = false;
        for (var offset = 0; offset < position; offset++)
        {
            var leftIndex = position - offset - 1;
            var rightIndex = position + offset;

            if (leftIndex < 0 || rightIndex >= value.Length)
            {
                break;
            }

            if (value[leftIndex] == value[rightIndex])
            {
                continue;
            }

            if (potentialSmudge)
            {
                // could only be a smudge if it's wrong in one place
                return false;
            }

            potentialSmudge = true;
        }

        return potentialSmudge;
    }

    private static string[][] GetGrids(string input)
    {
        var lines = input.GetLines(StringSplitOptions.TrimEntries);
        var grids = new List<string[]>();

        var grid = new List<string>();
        foreach (var line in lines)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                grid.Add(line);
                continue;
            }

            grids.Add(grid.ToArray());
            grid.Clear();
        }

        if (grid.Any())
        {
            grids.Add(grid.ToArray());
        }

        return grids.ToArray();
    }
}
