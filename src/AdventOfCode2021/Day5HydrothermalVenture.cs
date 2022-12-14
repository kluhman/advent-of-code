using System.Collections;
using AdventOfCode.Core;

namespace AdventOfCode2021;

public class Day5HydrothermalVenture : IChallenge
{
    public int ChallengeId => 5;

    public object SolvePart1(string input) => input
        .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
        .Select(Line.Parse)
        .Where(line => line.IsStraight())
        .SelectMany(line => line)
        .GroupBy(point => point)
        .Count(group => group.Count() > 1);

    public object SolvePart2(string input) => input
        .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
        .Select(Line.Parse)
        .SelectMany(line => line)
        .GroupBy(point => point)
        .Count(group => group.Count() > 1);

    public record Point(int X, int Y)
    {
        public static Point Parse(string input)
        {
            var parts = input.Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var x = int.Parse(parts[0]);
            var y = int.Parse(parts[1]);

            return new Point(x, y);
        }
    }

    public record Line(Point Start, Point End) : IEnumerable<Point>
    {
        public IEnumerator<Point> GetEnumerator() => GeneratePoints().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public static Line Parse(string input)
        {
            var parts = input.Split("->", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var start = Point.Parse(parts[0]);
            var end = Point.Parse(parts[1]);

            return new Line(start, end);
        }

        public bool IsStraight() => Start.X == End.X || Start.Y == End.Y;

        private IEnumerable<Point> GeneratePoints()
        {
            yield return Start;

            var horizontalIncrement = Start.X != End.X ? 1 : 0;
            if (End.X < Start.X)
            {
                horizontalIncrement *= -1;
            }

            var verticalIncrement = Start.Y != End.Y ? 1 : 0;
            if (End.Y < Start.Y)
            {
                verticalIncrement *= -1;
            }

            var temp = Start;
            var xDifference = Math.Abs(End.X - Start.X);
            var yDifference = Math.Abs(End.Y - Start.Y);
            var steps = Math.Max(xDifference, yDifference) - 1;
            for (var i = 0; i < steps; i++)
            {
                temp = new Point(temp.X + horizontalIncrement, temp.Y + verticalIncrement);
                yield return temp;
            }

            yield return End;
        }
    }
}