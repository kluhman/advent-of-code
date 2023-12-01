using AdventOfCode.Core;
using AdventOfCode.Core.Extensions;

namespace AdventOfCode2022;

internal class Day3RucksackOrganization : IChallenge
{
    public int ChallengeId => 3;

    public object SolvePart1(string input)
    {
        var commonItems = new List<char>();
        var lines = input.GetLines();
        foreach (var line in lines)
        {
            var divider = line.Length / 2;
            var firstCompartment = new List<char>();
            var secondCompartment = new List<char>();

            for (var i = 0; i < line.Length; i++)
            {
                if (i < divider)
                {
                    firstCompartment.Add(line[i]);
                }
                else
                {
                    secondCompartment.Add(line[i]);
                }
            }

            commonItems.AddRange(firstCompartment.Intersect(secondCompartment));
        }

        return PrioritizeItems(commonItems);
    }

    public object SolvePart2(string input)
    {
        var badges = new List<char>();
        var lines = input.GetLines();
        for (var i = 0; i < lines.Length; i += 3)
        {
            var firstSack = lines[i + 0];
            var secondSack = lines[i + 1];
            var thirdSack = lines[i + 2];

            var commonItems = firstSack.Intersect(secondSack).Intersect(thirdSack);
            badges.AddRange(commonItems);
        }

        return PrioritizeItems(badges);
    }

    private static object PrioritizeItems(List<char> items)
    {
        var sum = 0;
        foreach (var item in items)
        {
            var (baseLetter, baseScore) = char.IsUpper(item)
                ? ('A', 27)
                : ('a', 1);

            sum += item - baseLetter + baseScore;
        }

        return sum;
    }
}
