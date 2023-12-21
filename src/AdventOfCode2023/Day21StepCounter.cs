using AdventOfCode.Core;
using AdventOfCode.Core.Extensions;
using AdventOfCode.Core.Models;

namespace AdventOfCode2023;

public class Day21StepCounter : IChallenge
{
    public int ChallengeId => 21;

    public object SolvePart1(string input)
    {
        var map = Parse(input, out var start);

        return FindPossiblePlots(map, start, 64, false);
    }

    public object SolvePart2(string input)
    {
        // the pattern is quadratic, so find the first 3 values and then interpolate
        // f(n) = number of spaces you can reach with n steps
        // f(n), f(n + mapLength), f(n + 2*mapLength) is a quadratic equation with an unknown quadratic coefficient
        // so calculate the first 3 values for x mapLengths (0, 1, 2), then use them to interpolate the quadratic co-efficient and solve for the goal steps
        // https://www.reddit.com/r/adventofcode/comments/18nevo3/comment/keaiiq7/?utm_source=share&utm_medium=web3x&utm_name=web3xcss&utm_term=1&utm_content=share_button
        var map = Parse(input, out var start);
        var maxSteps = 26501365L;
        var remainingSteps = maxSteps % map.Length;

        var a0 = FindPossiblePlots(map, start, remainingSteps, true);
        var a1 = FindPossiblePlots(map, start, remainingSteps + map.Length, true);
        var a2 = FindPossiblePlots(map, start, remainingSteps + map.Length * 2, true);

        // interpolate and solve using LaGrange
        var b0 = a0;
        var b1 = a1 - a0;
        var b2 = a2 - a1;
        var mapCopies = maxSteps / map.Length;

        return b0 + b1 * mapCopies + mapCopies * (mapCopies - 1) / 2 * (b2 - b1);
    }

    private static long FindPossiblePlots(PlotType[][] map, Point start, long maxSteps, bool reflectMap)
    {
        var possibleGardens = 0;
        var visited = new HashSet<(Point point, long steps)>();
        var queue = new Queue<(Point point, long steps)>(new[] { (start, 0L) });

        var height = map.Length;
        var width = map[0].Length;

        while (queue.TryDequeue(out var visit))
        {
            if (!visited.Add(visit))
            {
                continue;
            }

            if (visit.steps == maxSteps)
            {
                possibleGardens++;
                continue;
            }

            foreach (var point in visit.point.Adjacent)
            {
                if (IsOutOfBounds(point, width, height) && !reflectMap)
                {
                    continue;
                }

                var reflectedPoint = GetReflectedPoint(point, width, height);
                if (map[reflectedPoint.Y][reflectedPoint.X] != PlotType.Garden)
                {
                    continue;
                }

                queue.Enqueue((point, visit.steps + 1));
            }
        }

        return possibleGardens;
    }

    private static Point GetReflectedPoint(Point point, int width, int height)
    {
        if (point.X >= width)
        {
            point = point with { X = point.X % width };
        }

        if (point.X < 0)
        {
            point = point with { X = point.X % width == 0 ? 0 : width + point.X % width };
        }

        if (point.Y >= height)
        {
            point = point with { Y = point.Y % height };
        }

        if (point.Y < 0)
        {
            point = point with { Y = point.Y % height == 0 ? 0 : height + point.Y % height };
        }

        return point;
    }

    private static bool IsOutOfBounds(Point point, int width, int height) =>
        point.X < 0 ||
        point.X >= width ||
        point.Y < 0 ||
        point.Y >= height;

    private static PlotType[][] Parse(string input, out Point start)
    {
        var lines = input.GetLines();
        var map = new PlotType[lines.Length][];

        start = new Point(0, 0);
        for (var y = 0; y < lines.Length; y++)
        {
            var line = lines[y];
            map[y] = new PlotType[line.Length];
            for (var x = 0; x < line.Length; x++)
            {
                var value = line[x];
                map[y][x] = value is '.' or 'S'
                    ? PlotType.Garden
                    : PlotType.Rock;

                if (value == 'S')
                {
                    start = new Point(x, y);
                }
            }
        }

        return map;
    }

    private enum PlotType
    {
        Garden,
        Rock
    }
}
