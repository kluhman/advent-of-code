using System.Collections.Immutable;
using System.Text.RegularExpressions;
using AdventOfCode.Core;
using AdventOfCode.Core.Models;

namespace AdventOfCode2022;

public class Day15BeaconExclusionZone : IChallenge
{
    public int ChallengeId => 15;

    public object SolvePart1(string input)
    {
        var sensors = ParseSensors(input);
        var combinedRanges = GetCombinedRangesOnY(sensors, 2000000);
        return combinedRanges.Sum(range => range.End - range.Start);
    }

    /// <summary>
    ///     This solutions works on the basis that there will only ever be one remaining space in the allowed range that the
    ///     beacon will be.
    ///     The point the beacon is located at will be (sensorDistance + 1) away from at least 2 sensors, which means if we
    ///     draw lines representing
    ///     the outer boundary of each sensor, the intersections of those boundaries will give us all the possible locations
    ///     where 2 sensors boundaries meet.
    ///     Then if we filter to those within the allowed range, only one should remain.
    /// </summary>
    public object SolvePart2(string input)
    {
        var maxRange = new Range<int>(0, 4000000);
        var sensors = ParseSensors(input).ToImmutableArray();
        var boundaryLines = sensors.SelectMany(sensor => sensor.GetBoundaryLines()).ToList();

        var boundaryIntersections = boundaryLines
            .SelectMany(line1 => boundaryLines.Select(line1.GetIntersection))
            .Where(point => point is not null && maxRange.Contains(point.X) && maxRange.Contains(point.Y))
            .Distinct();

        var distressBeaconLocation = boundaryIntersections.Single(point => !sensors.Any(sensor => sensor.Covers(point!)))!;

        return distressBeaconLocation.X * 4000000L + distressBeaconLocation.Y;
    }

    private static IEnumerable<Sensor> ParseSensors(string input) => input
        .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
        .Select(Sensor.Parse);

    private static IEnumerable<Range<int>> GetCombinedRangesOnY(IEnumerable<Sensor> sensors, int y, Range<int>? clamp = null)
    {
        var ranges = from sensor in sensors
            let range = sensor.GetRangeOnY(y)
            where range is not null
            select clamp is null
                ? range
                : new Range<int>(int.Max(clamp.Start, range.Start), int.Min(clamp.End, range.End));

        foreach (var range in CombineRanges(ranges))
        {
            yield return range;
        }
    }

    private static IEnumerable<Range<int>> CombineRanges(IEnumerable<Range<int>> ranges)
    {
        var temp = default(Range<int>?);
        foreach (var range in ranges.OrderBy(x => x.Start))
        {
            if (temp is null)
            {
                temp = range;
                continue;
            }

            if (temp.Intersects(range))
            {
                //because we ordered all the ranges by start, we always know temp will have the earliest start
                temp = new Range<int>(temp.Start, int.Max(temp.End, range.End));
                continue;
            }

            yield return temp;

            temp = null;
        }

        if (temp is not null)
        {
            yield return temp;
        }
    }

    private record Point(int X, int Y)
    {
        public int DistanceTo(Point other)
        {
            var xSteps = Math.Abs(X - other.X);
            var ySteps = Math.Abs(Y - other.Y);
            return xSteps + ySteps;
        }
    }

    private record Line(Point Start, Point End)
    {
        public Point? GetIntersection(Line other)
        {
            // this solution is based on math found here:
            // https://github.com/chrisleow/advent-of-code/blob/main/2022/src/Day15.kt
            var c1 = CalculateC(this) ?? CalculateC(other);
            if (c1 is null)
            {
                return null;
            }

            var c2 = CalculateC(this, -1) ?? CalculateC(other, -1);
            if (c2 is null)
            {
                return null;
            }

            return new Point((c1.Value + c2.Value) / 2, (c1.Value - c2.Value) / 2);
        }

        private static int? CalculateC(Line line, int multiplier = 1)
        {
            var c1 = line.Start.X + line.Start.Y * multiplier;
            var c2 = line.End.X + line.End.Y * multiplier;

            return c1 == c2 ? c1 : null;
        }
    }

    private record Sensor(Point Location, Point ClosestBeacon)
    {
        public int Distance { get; } = Location.DistanceTo(ClosestBeacon);

        public Range<int>? GetRangeOnY(int y)
        {
            var min = Location.Y - Distance;
            var max = Location.Y + Distance;
            if (y < min || y > max)
            {
                return null;
            }

            var ySteps = Math.Abs(Location.Y - y);
            var xSteps = Distance - ySteps;
            return new Range<int>(Location.X - xSteps, Location.X + xSteps);
        }

        public IEnumerable<Line> GetBoundaryLines()
        {
            var boundaryDistance = Distance + 1;
            var left = Location with { X = Location.X - boundaryDistance };
            var right = Location with { X = Location.X + boundaryDistance };
            var down = Location with { Y = Location.Y - boundaryDistance };
            var up = Location with { Y = Location.Y + boundaryDistance };

            yield return new Line(left, up);
            yield return new Line(left, down);
            yield return new Line(right, up);
            yield return new Line(right, down);
        }

        public bool Covers(Point point) => Location.DistanceTo(point) <= Distance;

        public static Sensor Parse(string line)
        {
            var match = Regex.Match(line, @"Sensor at x=(?<sensorX>-?\d+), y=(?<sensorY>-?\d+): closest beacon is at x=(?<beaconX>-?\d+), y=(?<beaconY>-?\d+)");
            var sensorLocation = new Point(int.Parse(match.Groups["sensorX"].Value), int.Parse(match.Groups["sensorY"].Value));
            var beaconLocation = new Point(int.Parse(match.Groups["beaconX"].Value), int.Parse(match.Groups["beaconY"].Value));

            return new Sensor(sensorLocation, beaconLocation);
        }
    }
}