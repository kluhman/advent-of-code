using AdventOfCode.Core;

namespace AdventOfCode2022;

internal class Day6TuningTrouble : IChallenge
{
    public int ChallengeId => 6;

    public object SolvePart1(string input)
    {
        const int markerLength = 4;
        return FindMarker(input, markerLength);
    }

    public object SolvePart2(string input)
    {
        const int markerLength = 14;
        return FindMarker(input, markerLength);
    }

    private static object FindMarker(string input, int markerLength)
    {
        for (var index = markerLength - 1; index < input.Length; index++)
        {
            var marker = input.Substring(index - markerLength + 1, markerLength);
            if (marker.Distinct().Count() != markerLength)
            {
                continue;
            }

            return index + 1;
        }

        throw new Exception("Marker could not be found");
    }
}