using System.Collections.Immutable;
using AdventOfCode.Core;
using AdventOfCode.Core.Extensions;

namespace AdventOfCode2023;

public class Day9MirageMaintenance : IChallenge
{
    public int ChallengeId => 9;

    public object SolvePart1(string input) => input
        .GetLines()
        .Select(line => line.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        .Select(values => new Reading(values.Select(int.Parse)))
        .Sum(reading => reading.ExtrapolateNextValue());

    public object SolvePart2(string input) => input
        .GetLines()
        .Select(line => line.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        .Select(values => new Reading(values.Select(int.Parse).Reverse()))
        .Sum(reading => reading.ExtrapolateNextValue());

    private class Reading
    {
        public Reading(IEnumerable<int> values)
        {
            Values = values.ToImmutableArray();
        }

        public IReadOnlyList<int> Values { get; }

        public int ExtrapolateNextValue() => ExtrapolateNextValue(Values);

        private static int ExtrapolateNextValue(IReadOnlyList<int> values)
        {
            if (!CanExtrapolateFurther(values))
            {
                return 0;
            }

            var differences = GetDifferences(values);
            return values[^1] + ExtrapolateNextValue(differences);
        }

        private static bool CanExtrapolateFurther(IReadOnlyList<int> values) => values.Any() && values.Any(x => x != 0);

        private static IReadOnlyList<int> GetDifferences(IReadOnlyList<int> values)
        {
            var differences = new List<int>();
            for (var index = 0; index < values.Count - 1; index++)
            {
                differences.Add(values[index + 1] - values[index]);
            }

            return differences;
        }
    }
}
