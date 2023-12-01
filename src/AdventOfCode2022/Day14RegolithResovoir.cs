using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using AdventOfCode.Core;
using AdventOfCode.Core.Extensions;
using AdventOfCode.Core.Models;

namespace AdventOfCode2022;

public class Day14RegolithResovoir : IChallenge
{
    public int ChallengeId => 14;

    public object SolvePart1(string input)
    {
        var rocks = ParseLineSegments(input).SelectMany(line => line).ToHashSet();
        var verticalBounds = new Range<int>(0, rocks.Max(x => x.Row));
        var horizontalBounds = rocks.ToRange(x => x.Column)!;

        var cave = new Cave(rocks, horizontalBounds, verticalBounds, false);
        return CalculateSandDropped(cave);
    }

    public object SolvePart2(string input)
    {
        var rocks = ParseLineSegments(input).SelectMany(line => line).ToHashSet();
        var verticalBounds = new Range<int>(0, rocks.Max(x => x.Row) + 2);
        var horizontalBounds = new Range<int>(int.MinValue, int.MaxValue);
        var cave = new Cave(rocks, horizontalBounds, verticalBounds, true);

        return CalculateSandDropped(cave);
    }

    private static object CalculateSandDropped(Cave cave)
    {
        var grainsDropped = 0;
        while (cave.DropSand())
        {
            grainsDropped++;
        }

        Console.WriteLine(cave);
        return grainsDropped;
    }

    private static IEnumerable<Line> ParseLineSegments(string input)
    {
        var lines = input.GetLines();
        foreach (var line in lines)
        {
            var points = line.Split("->", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            for (var index = 0; index < points.Length - 1; index++)
            {
                var start = Point.Parse(points[index]);
                var end = Point.Parse(points[index + 1]);
                yield return new Line(start, end);
            }
        }
    }

    private record Point(int Column, int Row)
    {
        public static Point operator +(Point point, Move move) => new(point.Column + move.Columns, point.Row + move.Rows);

        public static Point Parse(string input)
        {
            var parts = input.Split(',');
            return new Point(int.Parse(parts[0]), int.Parse(parts[1]));
        }
    }

    private record Move(int Rows, int Columns)
    {
        public static readonly Move Down = new(1, 0);
        public static readonly Move DownLeft = new(1, -1);
        public static readonly Move DownRight = new(1, 1);
    }

    private record Line(Point Start, Point End) : IEnumerable<Point>
    {
        public IEnumerator<Point> GetEnumerator() => EnumeratePoints().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private IEnumerable<Point> EnumeratePoints()
        {
            yield return Start;

            var steps = Math.Max(Math.Abs(End.Column - Start.Column), Math.Abs(End.Row - Start.Row));
            var columnIncrement = Start.Column == End.Column ? 0 : 1;
            if (End.Column < Start.Column)
            {
                columnIncrement *= -1;
            }

            var rowIncrement = Start.Row == End.Row ? 0 : 1;
            if (End.Row < Start.Row)
            {
                rowIncrement *= -1;
            }

            var point = Start;
            for (var i = 0; i < steps; i++)
            {
                point = new Point(point.Column + columnIncrement, point.Row + rowIncrement);
                yield return point;
            }
        }
    }

    private class Cave
    {
        private readonly bool _hasFloor;
        private readonly Dictionary<Point, Formation> _map;
        private readonly Point _sandSource;

        public Cave(IEnumerable<Point> rocks, Range<int> horizontalBounds, Range<int> verticalBounds, bool hasFloor)
        {
            _map = rocks.ToDictionary(point => point, _ => Formation.Rock);
            _sandSource = new Point(500, 0);
            _hasFloor = hasFloor;

            HorizontalBounds = horizontalBounds;
            VerticalBounds = verticalBounds;
        }

        public Range<int> HorizontalBounds { get; }
        public Range<int> VerticalBounds { get; }

        public Formation this[Point point]
        {
            get
            {
                if (_hasFloor && point.Row == VerticalBounds.End)
                {
                    return Formation.Rock;
                }

                return _map.TryGetValue(point, out var formation) ? formation : Formation.Air;
            }
        }

        public bool DropSand()
        {
            if (this[_sandSource] != Formation.Air)
            {
                return false;
            }

            var position = _sandSource;
            while (MoveSand(position, out var nextPosition))
            {
                if (IsOutOfBounds(nextPosition))
                {
                    return false;
                }

                position = nextPosition;
            }

            _map[position] = Formation.Sand;
            return true;
        }

        public override string ToString()
        {
            var columns = _map.Keys.ToRange(x => x.Column);
            if (columns == null)
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            for (var row = VerticalBounds.Start; row <= VerticalBounds.End; row++)
            {
                for (var column = columns.Start; column <= columns.End; column++)
                {
                    var point = new Point(column, row);
                    builder.Append(this[point] switch
                    {
                        Formation.Air => '.',
                        Formation.Rock => '#',
                        Formation.Sand => 'o',
                        _ => throw new ArgumentException()
                    });
                }

                builder.AppendLine();
            }

            return builder.ToString();
        }

        private bool MoveSand(Point position, [NotNullWhen(true)] out Point? nextPosition)
        {
            nextPosition = default;
            var moves = new[] { Move.Down, Move.DownLeft, Move.DownRight };
            foreach (var move in moves)
            {
                nextPosition = position + move;
                if (IsOutOfBounds(nextPosition) || this[nextPosition] == Formation.Air)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsOutOfBounds(Point nextPosition) => !VerticalBounds.Contains(nextPosition.Row) || !HorizontalBounds.Contains(nextPosition.Column);
    }

    private enum Formation
    {
        Air,
        Rock,
        Sand
    }
}