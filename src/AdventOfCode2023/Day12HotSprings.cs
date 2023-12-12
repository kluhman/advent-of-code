using System.Collections.Immutable;
using System.Text.RegularExpressions;
using AdventOfCode.Core;
using AdventOfCode.Core.Extensions;

namespace AdventOfCode2023;

public partial class Day12HotSprings : IChallenge
{
    private static readonly Dictionary<string, long> Cache = new();

    public int ChallengeId => 12;

    public object SolvePart1(string input) => ParseConditionReports(input).Sum(FindPossibleSolutions);

    public object SolvePart2(string input) => ParseConditionReports(input)
        .Select(report => report.Expand(5))
        .Sum(FindPossibleSolutions);

    private static IReadOnlyCollection<ConditionReport> ParseConditionReports(string input) => input
        .GetLines()
        .Select(line =>
        {
            var split = line.Split(' ');
            var map = string.Join("?", split[0]);
            var checkSums = split[1].Split(',').Select(int.Parse).ToImmutableArray();

            return new ConditionReport(map, checkSums);
        })
        .ToImmutableArray();

    private static long FindPossibleSolutions(ConditionReport conditionReport)
    {
        var key = $"{conditionReport.Map} {string.Join(',', conditionReport.CheckSums)}";
        if (Cache.TryGetValue(key, out var value))
        {
            return value;
        }

        // placed more damage than the check sum allows
        if (conditionReport.KnownDamage > conditionReport.TotalDamage)
        {
            return 0;
        }

        // we have to assign more damage than we have places to put them
        if (conditionReport.UnknownDamage > conditionReport.UnknownSpaces)
        {
            return 0;
        }

        if (!conditionReport.CheckSums.Any())
        {
            return conditionReport.KnownDamage == conditionReport.TotalDamage
                ? 1
                : 0;
        }

        var map = conditionReport.Map;
        var groupSize = conditionReport.CheckSums[0];
        var remainingGroups = conditionReport.CheckSums[1..];

        var maxWindowStart = GetMaxWindowStart(map, groupSize, remainingGroups);

        var options = new List<ConditionReport>();
        for (var windowStart = 0; windowStart <= maxWindowStart; windowStart++)
        {
            var windowEnd = windowStart + groupSize;
            var window = map[windowStart..windowEnd];

            if (!IsWindowValid(map, window, windowStart, windowEnd))
            {
                continue;
            }

            // add one to the window to account for the space between groups
            var nextWindowStart = windowEnd + 1;
            var newMap = nextWindowStart >= map.Length
                ? string.Empty
                : map[nextWindowStart..];

            options.Add(new ConditionReport(newMap, remainingGroups));
        }

        Cache[key] = options.Sum(FindPossibleSolutions);
        return Cache[key];
    }

    private static int GetMaxWindowStart(string map, int groupSize, ImmutableArray<int> remainingGroups)
    {
        // required groups require the sum of damage in each group plus one space between the groups
        var dividersRequired = remainingGroups.Length;
        var spaceRequiredForRemainingGroups = dividersRequired + remainingGroups.Sum();

        // max possible starting point is the length of map - space for remaining groups - size of the current group
        var maxPossibleIndex = map.Length - spaceRequiredForRemainingGroups - groupSize;

        // because we're processing the groups in order, if a window isn't found before the first known damage, then the group must include the first known damage
        var firstDamageIndex = map.IndexOf('#');
        return firstDamageIndex >= 0
            ? int.Min(firstDamageIndex, maxPossibleIndex)
            : maxPossibleIndex;
    }

    private static bool IsWindowValid(string map, string window, int startIndex, int endIndex)
    {
        // window must be all unknown or known damage to fit this group
        if (!ValidWindowRegex().IsMatch(window))
        {
            return false;
        }

        // if the previous spot is damage, then this window won't work because it would become too long
        if (startIndex > 0 && map[startIndex - 1] == '#')
        {
            return false;
        }

        // if the next spot is damage, then this window won't work because it would become too long
        if (endIndex < map.Length && map[endIndex] == '#')
        {
            return false;
        }

        return true;
    }

    [GeneratedRegex(@"^(#|\?)+$")]
    private static partial Regex ValidWindowRegex();

    public record ConditionReport(string Map, ImmutableArray<int> CheckSums)
    {
        public int TotalDamage { get; } = CheckSums.Sum();
        public int KnownDamage { get; } = Map.Count(character => character == '#');
        public int UnknownSpaces { get; } = Map.Count(character => character == '?');
        public int UnknownDamage => TotalDamage - KnownDamage;

        public ConditionReport Expand(int repeat)
        {
            var newMap = string.Join('?', Enumerable.Repeat(Map, repeat));
            var checkSums = Enumerable.Repeat(CheckSums, repeat).SelectMany(checkSum => checkSum).ToImmutableArray();

            return new ConditionReport(newMap, checkSums);
        }
    }
}
