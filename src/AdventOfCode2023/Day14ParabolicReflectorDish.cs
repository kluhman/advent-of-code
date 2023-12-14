using AdventOfCode.Core;
using AdventOfCode.Core.Extensions;

namespace AdventOfCode2023;

public class Day14ParabolicReflectorDish : IChallenge
{
    private const char OpenSpace = '.';
    private const char RoundRock = 'O';
    private const char CubeRock = '#';

    public int ChallengeId => 14;

    public object SolvePart1(string input)
    {
        var layout = GetInitialLayout(input);
        layout = TiltNorth(layout);

        return FindNorthLoad(layout);
    }

    public object SolvePart2(string input)
    {
        var layout = GetInitialLayout(input);
        var cache = new Dictionary<string, int>();

        const int maxCycles = 1_000_000_000;

        for (var currentCycle = 0; currentCycle < maxCycles; currentCycle++)
        {
            cache[StringifyLayout(layout)] = currentCycle;

            layout = TiltNorth(layout);
            layout = TiltWest(layout);
            layout = TiltSouth(layout);
            layout = TiltEast(layout);

            if (cache.TryGetValue(StringifyLayout(layout), out var previousCycle))
            {
                // add one to account for the cycle we just completed
                var iterationsInCycle = currentCycle - previousCycle + 1;
                var remainingCycles = maxCycles - currentCycle;
                var iterations = remainingCycles / iterationsInCycle;

                currentCycle += iterations * iterationsInCycle;
            }
        }

        return FindNorthLoad(layout);
    }

    private int FindNorthLoad(char[][] initialLayout)
    {
        var load = 0;
        var height = initialLayout.Length;
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < initialLayout[y].Length; x++)
            {
                if (initialLayout[y][x] == RoundRock)
                {
                    load += height - y;
                }
            }
        }

        return load;
    }

    private static char[][] TiltNorth(char[][] layout)
    {
        var width = layout[0].Length;
        var height = layout.Length;
        var newLayout = Enumerable.Range(0, height).Select(_ => new char[width]).ToArray();

        for (var x = 0; x < width; x++)
        {
            var availableSpace = 0;
            for (var y = 0; y < height; y++)
            {
                var value = layout[y][x];
                switch (value)
                {
                    case OpenSpace:
                        newLayout[y][x] = OpenSpace;
                        availableSpace++;
                        break;
                    case CubeRock:
                        newLayout[y][x] = CubeRock;
                        availableSpace = 0;
                        break;
                    case RoundRock:
                    {
                        var position = y - availableSpace;
                        newLayout[y][x] = OpenSpace;
                        newLayout[position][x] = RoundRock;
                        break;
                    }
                }
            }
        }

        return newLayout;
    }

    private static char[][] TiltSouth(char[][] layout)
    {
        var width = layout[0].Length;
        var height = layout.Length;
        var newLayout = Enumerable.Range(0, height).Select(_ => new char[width]).ToArray();

        for (var x = 0; x < width; x++)
        {
            var availableSpace = 0;
            for (var y = height - 1; y >= 0; y--)
            {
                var value = layout[y][x];
                switch (value)
                {
                    case OpenSpace:
                        newLayout[y][x] = OpenSpace;
                        availableSpace++;
                        break;
                    case CubeRock:
                        newLayout[y][x] = CubeRock;
                        availableSpace = 0;
                        break;
                    case RoundRock:
                    {
                        var position = y + availableSpace;
                        newLayout[y][x] = OpenSpace;
                        newLayout[position][x] = RoundRock;
                        break;
                    }
                }
            }
        }

        return newLayout;
    }

    private static char[][] TiltEast(char[][] layout)
    {
        var width = layout[0].Length;
        var height = layout.Length;

        var newLayout = Enumerable.Range(0, height).Select(_ => new char[width]).ToArray();

        for (var y = 0; y < height; y++)
        {
            var availableSpace = 0;
            var line = layout[y];
            for (var x = width - 1; x >= 0; x--)
            {
                var value = line[x];
                switch (value)
                {
                    case OpenSpace:
                        newLayout[y][x] = OpenSpace;
                        availableSpace++;
                        break;
                    case CubeRock:
                        newLayout[y][x] = CubeRock;
                        availableSpace = 0;
                        break;
                    case RoundRock:
                    {
                        var position = x + availableSpace;
                        newLayout[y][x] = OpenSpace;
                        newLayout[y][position] = RoundRock;
                        break;
                    }
                }
            }
        }

        return newLayout;
    }

    private static char[][] TiltWest(char[][] layout)
    {
        var width = layout[0].Length;
        var height = layout.Length;

        var newLayout = Enumerable.Range(0, height).Select(_ => new char[width]).ToArray();

        for (var y = 0; y < height; y++)
        {
            var availableSpace = 0;
            var line = layout[y];
            for (var x = 0; x < width; x++)
            {
                var value = line[x];
                switch (value)
                {
                    case OpenSpace:
                        newLayout[y][x] = OpenSpace;
                        availableSpace++;
                        break;
                    case CubeRock:
                        newLayout[y][x] = CubeRock;
                        availableSpace = 0;
                        break;
                    case RoundRock:
                    {
                        var position = x - availableSpace;
                        newLayout[y][x] = OpenSpace;
                        newLayout[y][position] = RoundRock;
                        break;
                    }
                }
            }
        }

        return newLayout;
    }

    private static string StringifyLayout(char[][] layout) => string.Join("\n", layout.Select(line => new string(line)));

    private static char[][] GetInitialLayout(string input)
    {
        return input.GetLines().Select(x => x.ToCharArray()).ToArray();
    }
}
