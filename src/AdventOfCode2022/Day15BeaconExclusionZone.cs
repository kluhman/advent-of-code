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

    public object SolvePart2(string input)
    {
        var maxRange = new Range<int>(0, 4000000);
        var sensors = ParseSensors(input).ToImmutableArray();

        var tuningFrequency = 0L;
        Parallel.For(maxRange.Start, maxRange.End + 1, y =>
        {
            var sensorXRanges = GetCombinedRangesOnY(sensors, y, maxRange).ToList();
            if (sensorXRanges.Count < 1)
            {
                return;
            }

            var firstRange = sensorXRanges.First();
            if (firstRange == maxRange)
            {
                return;
            }

            var x = (long)(firstRange.Start == maxRange.Start
                ? firstRange.End + 1
                : firstRange.Start - 1);

            tuningFrequency = x * 4000000 + y;
        });

        return tuningFrequency;
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

    private record Point(int X, int Y);

    private record Sensor(Point Location, Point ClosestBeacon)
    {
        public int Distance { get; } = CalculateDistance(Location, ClosestBeacon);

        public static Sensor Parse(string line)
        {
            var match = Regex.Match(line, @"Sensor at x=(?<sensorX>-?\d+), y=(?<sensorY>-?\d+): closest beacon is at x=(?<beaconX>-?\d+), y=(?<beaconY>-?\d+)");
            var sensorLocation = new Point(int.Parse(match.Groups["sensorX"].Value), int.Parse(match.Groups["sensorY"].Value));
            var beaconLocation = new Point(int.Parse(match.Groups["beaconX"].Value), int.Parse(match.Groups["beaconY"].Value));

            return new Sensor(sensorLocation, beaconLocation);
        }

        private static int CalculateDistance(Point point1, Point point2)
        {
            var xSteps = Math.Abs(point1.X - point2.X);
            var ySteps = Math.Abs(point1.Y - point2.Y);
            return xSteps + ySteps;
        }

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
    }
}