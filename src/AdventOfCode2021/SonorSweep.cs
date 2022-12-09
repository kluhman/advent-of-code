using AdventOfCode.Core;

namespace AdventOfCode2021;

internal class SonorSweep : IChallenge
{
    public int ChallengeId => 1;

    public object SolvePart1(string input)
    {
        var distances = input
            .Split("\n", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)
            .ToArray();

        var increases = 0;
        for (var index = 1; index < distances.Length; index++)
        {
            if (distances[index] > distances[index - 1])
            {
                increases++;
            }
        }

        return increases;
    }

    public object SolvePart2(string input)
    {
        const int windowSize = 3;
        var distances = input
            .Split("\n", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)
            .ToArray();

        var increases = 0;
        var previousWindow = 0;
        for (var index = 0; index <= distances.Length - windowSize; index++)
        {
            var window = GetCurrentWindow(distances, index, windowSize);
            if (index > 0 && window > previousWindow)
            {
                increases++;
            }

            previousWindow = window;
        }

        return increases;
    }

    private static int GetCurrentWindow(int[] distances, int startIndex, int windowSize)
    {
        var sum = 0;
        for (var i = 0; i < windowSize; i++)
        {
            sum += distances[startIndex + i];
        }

        return sum;
    }
}