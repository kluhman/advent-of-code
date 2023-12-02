using System.Collections.Immutable;
using System.Text.RegularExpressions;
using AdventOfCode.Core;
using AdventOfCode.Core.Extensions;

namespace AdventOfCode2023;

public class DayTwoCubeConundrum : IChallenge
{
    public int ChallengeId => 2;

    public object SolvePart1(string input)
    {
        var supply = new Sample(12, 14, 13);
        return input
            .GetLines()
            .Select(Game.Parse)
            .Where(game => game.Samples.All(sample => sample.Red <= supply.Red && sample.Blue <= supply.Blue && sample.Green <= supply.Green))
            .Sum(x => x.Id);
    }

    public object SolvePart2(string input)
    {
        return input
            .GetLines()
            .Select(Game.Parse)
            .Select(game => game.CalculateMinimumSample())
            .Sum(x => x.Power);
    }

    private record Sample(int Red, int Blue, int Green)
    {
        public int Power => Red * Blue * Green;

        public static Sample Parse(string line)
        {
            var (red, blue, green) = (0, 0, 0);
            var matches = Regex.Matches(line, @"(?<count>\d+)\s(?<color>blue|red|green)", RegexOptions.Compiled);
            foreach (Match match in matches)
            {
                switch (match.Groups["color"].Value)
                {
                    case "red":
                        red = int.Parse(match.Groups["count"].Value);
                        break;
                    case "blue":
                        blue = int.Parse(match.Groups["count"].Value);
                        break;
                    case "green":
                        green = int.Parse(match.Groups["count"].Value);
                        break;
                }
            }

            return new Sample(red, blue, green);
        }
    }

    private record Game(int Id, IReadOnlyCollection<Sample> Samples)
    {
        public Sample CalculateMinimumSample() => Samples.Aggregate(new Sample(0, 0, 0),
            (current, sample) => new Sample(int.Max(current.Red, sample.Red), int.Max(current.Blue, sample.Blue), int.Max(current.Green, sample.Green)));

        public static Game Parse(string line)
        {
            var split = line.Split(':', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var gameTitle = split[0];
            var sampleStrings = split[1].Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            var gameId = int.Parse(gameTitle.Replace("Game ", string.Empty));
            var samples = sampleStrings.Select(Sample.Parse).ToImmutableArray();
            return new Game(gameId, samples);
        }
    }
}
