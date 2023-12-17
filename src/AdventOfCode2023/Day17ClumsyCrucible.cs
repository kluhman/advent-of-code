using System.Diagnostics.CodeAnalysis;
using AdventOfCode.Core;
using AdventOfCode.Core.Extensions;
using AdventOfCode.Core.PathFinding;

namespace AdventOfCode2023;

public class Day17ClumsyCrucible : IChallenge
{
    public int ChallengeId => 17;

    public object SolvePart1(string input)
    {
        var map = ParseMap(input);
        var graph = GetGraph(map);
        var start = new Position(0, 0, null);
        var end = new Position(map[0].Length - 1, map.Length - 1, null);

        return PathFinder.FindShortestPath(graph, start, end);
    }

    public object SolvePart2(string input)
    {
        var map = ParseMap(input);
        var graph = GetGraph(map, 4, 10);
        var start = new Position(0, 0, null);
        var end = new Position(map[0].Length - 1, map.Length - 1, null);

        return PathFinder.FindShortestPath(graph, start, end);
    }

    private static WeightedGraph<Position> GetGraph(int[][] map, int minMovement = 1, int maxMovement = 3)
    {
        var nodes = Enumerable
            .Range(0, map.Length)
            .SelectMany(y => Enumerable
                .Range(0, map[y].Length)
                .SelectMany(x =>
                {
                    if (x == 0 && y == 0)
                    {
                        return new[] { new Position(0, 0, null) };
                    }

                    if (x == map[y].Length - 1 && y == map.Length - 1)
                    {
                        return new[] { new Position(x, y, null) };
                    }

                    return new[]
                    {
                        new Position(x, y, Direction.Horizontal),
                        new Position(x, y, Direction.Vertical)
                    };
                }));

        return new WeightedGraph<Position>(nodes, node => GetEdges(map, node, minMovement, maxMovement));
    }

    private static IEnumerable<WeightedEdge<Position>> GetEdges(int[][] map, Position position, int minMovement, int maxMovement)
    {
        if (position.Direction is null or Direction.Horizontal)
        {
            var (upCost, downCost) = (0, 0);
            for (var yOffset = 1; yOffset <= maxMovement; yOffset++)
            {
                var up = position with { Y = position.Y - yOffset, Direction = Direction.Vertical };
                if (TryAddEdge(map, minMovement, position, up, yOffset, ref upCost, out var edge))
                {
                    yield return edge;
                }

                var down = position with { Y = position.Y + yOffset, Direction = Direction.Vertical };
                if (TryAddEdge(map, minMovement, position, down, yOffset, ref downCost, out edge))
                {
                    yield return edge;
                }
            }
        }

        if (position.Direction is null or Direction.Vertical)
        {
            var (leftCost, rightCost) = (0, 0);
            for (var xOffset = 1; xOffset <= maxMovement; xOffset++)
            {
                var left = position with { X = position.X - xOffset, Direction = Direction.Horizontal };
                if (TryAddEdge(map, minMovement, position, left, xOffset, ref leftCost, out var edge))
                {
                    yield return edge;
                }

                var right = position with { X = position.X + xOffset, Direction = Direction.Horizontal };
                if (TryAddEdge(map, minMovement, position, right, xOffset, ref rightCost, out edge))
                {
                    yield return edge;
                }
            }
        }
    }

    private static bool TryAddEdge(int[][] map, int minMovement, Position from, Position to, int offset, ref int cost,
        [NotNullWhen(true)] out WeightedEdge<Position>? edge)
    {
        edge = null;
        if (!IsInBounds(map, to))
        {
            return false;
        }

        cost += map[to.Y][to.X];
        if (offset < minMovement)
        {
            return false;
        }

        if (to.X == map[to.Y].Length - 1 && to.Y == map.Length - 1)
        {
            to = to with { Direction = null };
        }

        edge = new WeightedEdge<Position>(from, to, cost);
        return true;
    }

    private static bool IsInBounds(int[][] map, Position position)
    {
        var withinWidth = position.X >= 0 && position.X < map[0].Length;
        var withinHeight = position.Y >= 0 && position.Y < map.Length;

        return withinWidth && withinHeight;
    }

    private static int[][] ParseMap(string input) => input
        .GetLines()
        .Select(line => line.Select(x => int.Parse(x.ToString())).ToArray())
        .ToArray();

    private enum Direction
    {
        Horizontal,
        Vertical
    }

    private record Position(int X, int Y, Direction? Direction);
}
