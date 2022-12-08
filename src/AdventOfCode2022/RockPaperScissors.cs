using AdventOfCode.Core;
using System.Reflection;

namespace AdventOfCode2022
{
    internal class RockPaperScissors : IChallenge
    {
        public int ChallengeId => 2;

        public object SolvePart1(string input)
        {
            var points = 0;
            var lines = input.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var selections = line.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (selections.Length != 2)
                {
                    throw new Exception($"Line '{line}' is not a valid strategy");
                }

                if (!_opponentSelections.TryGetValue(selections[0], out var opponent))
                {
                    throw new Exception($"'{selections[0]}' is not a valid option for opponent");
                }

                if (!_part1SelfSelections.TryGetValue(selections[1], out var self))
                {
                    throw new Exception($"'{selections[1]}' is not a valid option for self");
                }

                points += self.Points;
                if (self.Beats == opponent.Name)
                {
                    points += Win.Points;
                }
                else if (opponent.Beats == self.Name)
                {
                    points += Loss.Points;
                }
                else
                {
                    points += Draw.Points;
                }
            }

            return points;
        }

        public object SolvePart2(string input)
        {
            var points = 0;
            var lines = input.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var selections = line.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (selections.Length != 2)
                {
                    throw new Exception($"Line '{line}' is not a valid strategy");
                }

                if (!_opponentSelections.TryGetValue(selections[0], out var opponentSelection))
                {
                    throw new Exception($"'{selections[0]}' is not a valid option for opponent");
                }

                if (!_part2SelfSelections.TryGetValue(selections[1], out var outcome))
                {
                    throw new Exception($"'{selections[1]}' is not a valid option for self");
                }

                var (winner, loser) = GetOptions(opponentSelection);

                points += outcome.Points;
                if (outcome == Draw)
                {
                    points += opponentSelection.Points;
                }
                else if (outcome == Loss)
                {
                    points += loser.Points;
                }
                else
                {
                    points += winner.Points;
                }
            }

            return points;
        }

        private static (Option winner, Option loser) GetOptions(Option opponentSelection)
        {
            if (_cache.TryGetValue(opponentSelection.Name, out var result))
            {
                return result;
            }

            var otherOptions = typeof(RockPaperScissors)
                .GetFields(BindingFlags.NonPublic | BindingFlags.Static)
                .Where(x => x.FieldType == typeof(Option))
                .Select(x => (Option)x.GetValue(null)!)
                .Where(x => x != opponentSelection)
                .ToArray();

            var winner = otherOptions.Single(x => x.Beats == opponentSelection.Name);
            var loser = otherOptions.Single(x => opponentSelection.Beats == x.Name);

            result = (winner, loser);
            _cache.Add(opponentSelection.Name, result);
            
            return result;
        }

        private record Option(string Name, int Points, string Beats);
        private record Outcome(string Name, int Points);

        private static readonly Option Rock = new(nameof(Rock), 1, nameof(Scissors));
        private static readonly Option Paper = new(nameof(Paper), 2, nameof(Rock));
        private static readonly Option Scissors = new(nameof(Scissors), 3, nameof(Paper));

        private static readonly Outcome Loss = new(nameof(Loss), 0);
        private static readonly Outcome Draw = new(nameof(Draw), 3);
        private static readonly Outcome Win = new(nameof(Win), 6);

        private static readonly IReadOnlyDictionary<string, Option> _opponentSelections = new Dictionary<string, Option>
        {
            { "A", Rock },
            { "B", Paper },
            { "C", Scissors },
        };

        private static readonly IReadOnlyDictionary<string, Option> _part1SelfSelections = new Dictionary<string, Option>
        {
            { "X", Rock },
            { "Y", Paper },
            { "Z", Scissors },
        };

        private static readonly IReadOnlyDictionary<string, Outcome> _part2SelfSelections = new Dictionary<string, Outcome>
        {
            { "X", Loss },
            { "Y", Draw},
            { "Z", Win},
        };

        private static readonly Dictionary<string, (Option winner, Option loser)> _cache = new();
    }
}
