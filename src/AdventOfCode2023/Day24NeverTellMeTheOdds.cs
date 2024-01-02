using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using AdventOfCode.Core;
using AdventOfCode.Core.Extensions;
using AdventOfCode.Core.Models;

namespace AdventOfCode2023;

public class Day24NeverTellMeTheOdds : IChallenge
{
    public int ChallengeId => 24;

    public object SolvePart1(string input)
    {
        var hail = input
            .GetLines()
            .Select(line =>
            {
                var splits = line.Split('@', StringSplitOptions.TrimEntries);
                var positions = splits[0].Split(',', StringSplitOptions.TrimEntries).Select(long.Parse).ToArray();
                var velocity = splits[1].Split(',', StringSplitOptions.TrimEntries).Select(long.Parse).ToArray();

                return new Hail(new Point3D(positions[0], positions[1], positions[2]), new Velocity(velocity[0], velocity[1], velocity[2]));
            })
            .ToImmutableArray();

        var count = 0;
        var testArea = new Range<double>(200000000000000, 400000000000000);
        for (var index = 0; index < hail.Length - 1; index++)
        {
            for (var otherIndex = index + 1; otherIndex < hail.Length; otherIndex++)
            {
                if (otherIndex == index)
                {
                    continue;
                }

                var piece1 = hail[index];
                var piece2 = hail[otherIndex];
                if (Intersects(piece1, piece2, out var intersection) &&
                    testArea.Contains(intersection.X) &&
                    testArea.Contains(intersection.Y) &&
                    IsInFuture(piece1, intersection) &&
                    IsInFuture(piece2, intersection))
                {
                    count++;
                }
            }
        }

        return count;
    }

    public object SolvePart2(string input) => throw new NotImplementedException();

    private bool IsInFuture(Hail hail, Intersection intersection)
    {
        bool IsPositive(double num) => num > 0;
        var xDiff = intersection.X - hail.Position.X;
        var yDiff = intersection.Y - hail.Position.Y;

        if (xDiff == 0 || yDiff == 0)
        {
            return false;
        }

        return IsPositive(xDiff) == IsPositive(hail.Velocity.X) &&
               IsPositive(yDiff) == IsPositive(hail.Velocity.Y);
    }

    private bool Intersects(Hail piece1, Hail piece2, [NotNullWhen(true)] out Intersection? intersection)
    {
        var p1 = piece1.Position;
        var p2 = piece1.Position + piece1.Velocity;

        var p3 = piece2.Position;
        var p4 = piece2.Position + piece2.Velocity;

        var denominator = (double)((p1.X - p2.X) * (p3.Y - p4.Y) - (p1.Y - p2.Y) * (p3.X - p4.X));
        if (denominator == 0)
        {
            intersection = null;
            return false;
        }

        var xNumerator = (p1.X * p2.Y - p1.Y * p2.X) * (p3.X - p4.X) - (p1.X - p2.X) * (p3.X * p4.Y - p3.Y * p4.X);
        var yNumerator = (p1.X * p2.Y - p1.Y * p2.X) * (p3.Y - p4.Y) - (p1.Y - p2.Y) * (p3.X * p4.Y - p3.Y * p4.X);

        intersection = new Intersection(xNumerator / denominator, yNumerator / denominator);
        return true;
    }

    private record Hail(Point3D Position, Velocity Velocity);

    private record Intersection(double X, double Y);

    private record Velocity(long X, long Y, long Z)
    {
        public static Point3D operator +(Point3D point, Velocity velocity) => new(point.X + velocity.X, point.Y + velocity.Y, point.Z + velocity.Z);
    }
}
