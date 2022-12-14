using AdventOfCode.Core;
using AdventOfCode.Core.Models;

namespace AdventOfCode2022;

internal class CampCleanup : IChallenge
{
    public int ChallengeId => 4;

    public object SolvePart1(string input)
    {
        var uselessAssignments = 0;
        var lines = input.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var line in lines)
        {
            var assignments = line.Split(',', 2);
            var assignment1 = assignments[0].Split('-', 2);
            var assignment2 = assignments[1].Split('-', 2);

            var firstRange = new Range<int>(int.Parse(assignment1[0]), int.Parse(assignment1[1]));
            var secondRange = new Range<int>(int.Parse(assignment2[0]), int.Parse(assignment2[1]));

            if (firstRange.Contains(secondRange) || secondRange.Contains(firstRange))
            {
                uselessAssignments++;
            }
        }

        return uselessAssignments;
    }

    public object SolvePart2(string input)
    {
        var overlappingAssignments = 0;
        var lines = input.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var line in lines)
        {
            var assignments = line.Split(',', 2);
            var assignment1 = assignments[0].Split('-', 2);
            var assignment2 = assignments[1].Split('-', 2);

            var firstRange = new Range<int>(int.Parse(assignment1[0]), int.Parse(assignment1[1]));
            var secondRange = new Range<int>(int.Parse(assignment2[0]), int.Parse(assignment2[1]));

            if (firstRange.Intersects(secondRange))
            {
                overlappingAssignments++;
            }
        }

        return overlappingAssignments;
    }
}
