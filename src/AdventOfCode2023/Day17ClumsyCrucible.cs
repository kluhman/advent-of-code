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
        return FindPath(map);
    }

    public object SolvePart2(string input)
    {
        throw new NotImplementedException();

        var map = ParseMap(input);
        return FindPath(map);
    }

    private int FindPath(int[][] map)
    {
        var end = new Position(map[0].Length - 1, map.Length - 1, null);
        var position = new Position(0, 0, null);
        var visits = new HashSet<Position>();
        var distance = new Dictionary<Position, int> { { position, 0 } };

        while (position.X != end.X || position.Y != end.Y)
        {
            visits.Add(position);

            foreach (var edge in GetEdges(map, position))
            {
                var newDistance = distance[position] + edge.Weight;
                if (!distance.TryGetValue(edge.To, out var oldDistance) || newDistance < oldDistance)
                {
                    distance[edge.To] = newDistance;
                }
            }

            var newPosition = distance
                .Keys
                .Where(key => !visits.Contains(key)).MinBy(key => distance[key]);

            if (newPosition is null)
            {
                break;
            }

            position = newPosition;
        }

        return distance.Keys.Where(key => key.X == end.X && key.Y == end.Y).Min(key => distance[key]);
    }

    private IEnumerable<WeightedEdge<Position>> GetEdges(int[][] map, Position position)
    {
        var (xMaxOffset, yMaxOffset) = position.Direction switch
        {
            Direction.Left => (0, 3),
            Direction.Right => (0, 3),
            Direction.Up => (3, 0),
            Direction.Down => (3, 0),
            null => (3, 3),
            _ => throw new ArgumentOutOfRangeException(nameof(position), position, null)
        };

        var edges = new List<WeightedEdge<Position>>();
        var (leftWeight, rightWeight) = (0, 0);
        for (var x = 1; x <= xMaxOffset; x++)
        {
            var leftX = position.X - x;
            if (leftX >= 0)
            {
                leftWeight += map[position.Y][leftX];
                var left = position with { X = leftX, Direction = Direction.Left };
                edges.Add(new WeightedEdge<Position>(position, left, leftWeight));
            }

            var rightX = position.X + x;
            if (rightX < map[position.Y].Length)
            {
                rightWeight += map[position.Y][rightX];
                var right = position with { X = rightX, Direction = Direction.Right };
                edges.Add(new WeightedEdge<Position>(position, right, rightWeight));
            }
        }

        var (upWeight, downWeight) = (0, 0);
        for (var y = 1; y <= yMaxOffset; y++)
        {
            var upY = position.Y - y;
            if (upY >= 0)
            {
                upWeight += map[upY][position.X];
                var up = position with { Y = upY, Direction = Direction.Up };
                edges.Add(new WeightedEdge<Position>(position, up, upWeight));
            }

            var downY = position.Y + y;
            if (downY < map.Length)
            {
                downWeight += map[downY][position.X];
                var down = position with { Y = downY, Direction = Direction.Down };
                edges.Add(new WeightedEdge<Position>(position, down, downWeight));
            }
        }

        return edges;
    }

    private static int[][] ParseMap(string input) => input
        .GetLines()
        .Select(line => line.Select(x => int.Parse(x.ToString())).ToArray())
        .ToArray();

    private enum Direction
    {
        Left,
        Right,
        Up,
        Down
    }

    private record Position(int X, int Y, Direction? Direction);
}
