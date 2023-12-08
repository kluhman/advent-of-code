using System.Collections.Immutable;
using System.Text.RegularExpressions;
using AdventOfCode.Core;
using AdventOfCode.Core.Extensions;
using AdventOfCode.Core.Models;

namespace AdventOfCode2023;

public class Day6BoatRacing : IChallenge
{
    public int ChallengeId => 6;

    public object SolvePart1(string input)
    {
        var lines = input.GetLines();
        var times = SplitValues(lines[0]);
        var recordDistances = SplitValues(lines[1]);

        var value = 1L;
        for (var race = 0; race < times.Count; race++)
        {
            var time = times[race];
            var recordDistance = recordDistances[race];

            value *= CalculateWinningOptions(time, recordDistance);
        }

        return value;
    }

    public object SolvePart2(string input)
    {
        var lines = input.GetLines();
        var time = GetNumber(lines[0]);
        var recordDistance = GetNumber(lines[1]);

        return CalculateWinningOptions(time, recordDistance);
    }

    private static long CalculateWinningOptions(long time, long recordDistance)
    {
        // add one to current record so we only get charge times that exceed the record
        var chargeTimes = FindChargeTimeBounds(time, recordDistance + 1);

        // add one to make difference inclusive of start and end
        return chargeTimes.End - chargeTimes.Start + 1;
    }

    private static Range<long> FindChargeTimeBounds(long time, long recordDistance)
    {
        // charge + drive = time
        // charge * drive = distance
        // drive = time - charge
        // charge * (time - charge) = distance
        // (charge * time) - charge^2 = distance
        // 0 = charge^2 - (charge * time) + distance

        // solve quadratic formula
        var (charge1, charge2) = ExtraMath.QuadraticFormula(1, -time, recordDistance);

        // formula may produce fractional numbers, but you can't charge for partial seconds, so ceil and floor the bounds
        long lowerBound, upperBound;
        if (charge1 > charge2)
        {
            lowerBound = (long)Math.Ceiling(charge2);
            upperBound = (long)Math.Floor(charge1);
        }
        else
        {
            lowerBound = (long)Math.Ceiling(charge1);
            upperBound = (long)Math.Floor(charge2);
        }

        return new Range<long>(lowerBound, upperBound);
    }

    private static IReadOnlyList<long> SplitValues(string line) => line
        .Split(':', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)[1]
        .Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
        .Select(long.Parse)
        .ToImmutableArray();

    private static long GetNumber(string line)
    {
        var joined = string.Join(string.Empty, Regex
            .Matches(line, @"\d+", RegexOptions.Compiled)
            .Select(x => x.Value));

        return long.Parse(joined);
    }
}
