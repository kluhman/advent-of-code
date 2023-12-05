using System.Collections.Immutable;
using AdventOfCode.Core;
using AdventOfCode.Core.Extensions;
using AdventOfCode.Core.Models;

namespace AdventOfCode2023;

public class Day5SeedFertilizer : IChallenge
{
    public int ChallengeId => 5;

    public object SolvePart1(string input)
    {
        var lines = input.GetLines(StringSplitOptions.TrimEntries).ToImmutableArray();
        var seeds = lines.First().Split(':', StringSplitOptions.TrimEntries)[1].Split(' ').Select(long.Parse).ToList();
        var almanac = SeedAlmanac.Parse(lines);

        return seeds
            .Select(seed => almanac.GetLocation(seed))
            .Min();
    }

    public object SolvePart2(string input)
    {
        var lines = input.GetLines(StringSplitOptions.TrimEntries).ToImmutableArray();
        var almanac = SeedAlmanac.Parse(lines);

        return GetSeedRanges(lines.First())
            .Select(seedRange => almanac.GetLowestLocation(seedRange))
            .Min();
    }

    private static IEnumerable<Range<long>> GetSeedRanges(string line)
    {
        var numbers = line.Split(':', StringSplitOptions.TrimEntries)[1].Split(' ').Select(long.Parse).ToList();
        for (var index = 0; index < numbers.Count; index += 2)
        {
            var start = numbers[index];
            var length = numbers[index + 1];
            yield return new Range<long>(start, start + length - 1);
        }
    }

    private class SeedAlmanac
    {
        public required Map SeedToSoil { get; init; }
        public required Map SoilToFertilizer { get; init; }
        public required Map FertilizerToWater { get; init; }
        public required Map WaterToLight { get; init; }
        public required Map LightToTemperature { get; init; }
        public required Map TemperatureToHumidity { get; init; }
        public required Map HumidityToLocation { get; init; }

        public long GetLocation(long seed)
        {
            var soil = SeedToSoil.Find(seed);
            var fertilizer = SoilToFertilizer.Find(soil);
            var water = FertilizerToWater.Find(fertilizer);
            var light = WaterToLight.Find(water);
            var temperature = LightToTemperature.Find(light);
            var humidity = TemperatureToHumidity.Find(temperature);
            var location = HumidityToLocation.Find(humidity);

            return location;
        }

        public long GetLowestLocation(Range<long> seedRange) => SeedToSoil
            .Find(seedRange)
            .SelectMany(SoilToFertilizer.Find)
            .SelectMany(FertilizerToWater.Find)
            .SelectMany(WaterToLight.Find)
            .SelectMany(LightToTemperature.Find)
            .SelectMany(TemperatureToHumidity.Find)
            .SelectMany(HumidityToLocation.Find)
            .Min(x => x.Start);

        public static SeedAlmanac Parse(ImmutableArray<string> lines) =>
            new()
            {
                SeedToSoil = Map.Parse(lines[lines.IndexOf("seed-to-soil map:")..]),
                SoilToFertilizer = Map.Parse(lines[lines.IndexOf("soil-to-fertilizer map:")..]),
                FertilizerToWater = Map.Parse(lines[lines.IndexOf("fertilizer-to-water map:")..]),
                WaterToLight = Map.Parse(lines[lines.IndexOf("water-to-light map:")..]),
                LightToTemperature = Map.Parse(lines[lines.IndexOf("light-to-temperature map:")..]),
                TemperatureToHumidity = Map.Parse(lines[lines.IndexOf("temperature-to-humidity map:")..]),
                HumidityToLocation = Map.Parse(lines[lines.IndexOf("humidity-to-location map:")..])
            };
    }

    private class Map
    {
        public Map(IEnumerable<MapRange> ranges)
        {
            Ranges = ExpandRanges(ranges);
        }

        public IReadOnlyCollection<MapRange> Ranges { get; }

        public long Find(long value)
        {
            var range = Ranges.Single(x => x.Source.Contains(value));
            var offset = value - range.Source.Start;
            return range.Destination.Start + offset;
        }

        public IEnumerable<Range<long>> Find(Range<long> range) => Ranges
            .Where(x => x.Source.Intersects(range))
            .Select(mapRange => MapDestinationRange(mapRange, mapRange.Source.GetIntersection(range)!));

        public static Map Parse(ImmutableArray<string> lines)
        {
            var end = lines.IndexOf(string.Empty);
            var ranges = lines[1..end]
                .Select(MapRange.Parse)
                .ToList();

            return new Map(ranges);
        }

        private static IReadOnlyCollection<MapRange> ExpandRanges(IEnumerable<MapRange> ranges)
        {
            var start = 0L;
            var expanded = new List<MapRange>();
            foreach (var mapRange in ranges.OrderBy(x => x.Source.Start))
            {
                var range = mapRange.Source;
                if (start == range.Start)
                {
                    expanded.Add(mapRange);
                    start = range.End + 1;
                }
                else
                {
                    var length = range.Start - start;
                    expanded.Add(new MapRange(start, start, length));
                    expanded.Add(mapRange);
                    start = range.End + 1;
                }
            }

            expanded.Add(new MapRange(start, start, long.MaxValue - start));

            return expanded;
        }

        private static Range<long> MapDestinationRange(MapRange mapRange, Range<long> input)
        {
            var offset = mapRange.Destination.Start - mapRange.Source.Start;
            return new Range<long>(input.Start + offset, input.End + offset);
        }
    }

    private class MapRange
    {
        public MapRange(long sourceStart, long destinationStart, long rangeLength)
        {
            Source = new Range<long>(sourceStart, sourceStart + rangeLength - 1);
            Destination = new Range<long>(destinationStart, destinationStart + rangeLength - 1);
        }

        public Range<long> Source { get; }
        public Range<long> Destination { get; }

        public static MapRange Parse(string line)
        {
            var values = line
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(long.Parse)
                .ToImmutableArray();

            return new MapRange(values[1], values[0], values[2]);
        }
    }
}
