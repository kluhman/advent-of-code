using System.Collections.Immutable;
using AdventOfCode.Core;
using AdventOfCode.Core.Extensions;
using AdventOfCode.Core.Models;

namespace AdventOfCode2023;

public class Day23LongWalk : IChallenge
{
    public int ChallengeId => 23;

    public object SolvePart1(string input) => FindMaxWalkLength(input);

    public object SolvePart2(string input) =>
        // TODO: in order to process the number of paths created by climbing the slopes, need to compress the grid into a weighted graph to eliminate excess branches
        FindMaxWalkLength(input, true);

    private long FindMaxWalkLength(string input, bool canClimbSlopes = false)
    {
        var map = input.GetLines().Select(x => x.ToImmutableArray()).ToImmutableArray();
        var start = new Point(map[0].IndexOf('.'), 0);
        var end = new Point(map[^1].IndexOf('.'), map.Length - 1);

        var maxLength = 0;
        var stack = new Stack<(Point point, ImmutableHashSet<Point> visits)>(new[] { (start, ImmutableHashSet<Point>.Empty) });
        while (stack.TryPop(out var visit))
        {
            var (point, visits) = visit;
            if (point == end)
            {
                maxLength = int.Max(maxLength, visits.Count);
                continue;
            }

            var newVisits = visits.Add(point);
            var moves = GetValidMoves(map, point, canClimbSlopes).Where(move => !visits.Contains(move));
            foreach (var move in moves)
            {
                stack.Push((move, newVisits));
            }
        }

        return maxLength;
    }

    private static IEnumerable<Point> GetValidMoves(ImmutableArray<ImmutableArray<char>> map, Point point, bool canClimbSlopes)
    {
        bool IsInBounds(Point next) => next.X >= 0 &&
                                       next.X < map[0].Length &&
                                       next.Y >= 0 &&
                                       next.Y < map.Length;

        var value = map[point.Y][point.X];
        return value switch
        {
            '>' when !canClimbSlopes => new[] { point.Right },
            '<' when !canClimbSlopes => new[] { point.Left },
            '^' when !canClimbSlopes => new[] { point.Up },
            'v' when !canClimbSlopes => new[] { point.Down },
            _ => point.Adjacent.Where(next => IsInBounds(next) && map[next.Y][next.X] != '#').ToArray()
        };
    }
}
