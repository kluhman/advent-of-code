using AdventOfCode.Core;

namespace AdventOfCode2021
{
    internal class BinaryDiagnostic : IChallenge
    {
        public int ChallengeId => 3;

        public object SolvePart1(string input)
        {
            var gamma = "";
            var epsilon = "";
            var lines = input.Split("\n", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            var numberOfBits = lines[0].Length;
            for (int bit = 0; bit < numberOfBits; bit++)
            {
                int ones = 0;
                int zeros = 0;
                foreach (var line in lines)
                {
                    if(line[bit] == '0')
                    {
                        zeros++;
                    }
                    else
                    {
                        ones++;
                    }
                }

                gamma += ones > zeros ? '1' : '0';
                epsilon += ones > zeros ? '0' : '1';
            }

            return Convert.ToInt32(gamma, 2) * Convert.ToInt32(epsilon, 2);
        }

        public object SolvePart2(string input)
        {
            var lines = input.Split("\n", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            var oxygenRating = FindRating(lines.ToList(), (ones, zeros) => ones >= zeros ? '1' : '0');
            var co2Rating = FindRating(lines.ToList(), (ones, zeros) => ones >= zeros ? '0' : '1');

            return Convert.ToInt32(oxygenRating, 2) * Convert.ToInt32(co2Rating, 2);
        }

        private static string FindRating(List<string> values, Func<int, int, char> filterBuilder)
        {
            var numberOfBits = values[0].Length;
            for (int bit = 0; bit < numberOfBits; bit++)
            {
                int ones = 0;
                int zeros = 0;
                foreach (var line in values)
                {
                    if (line[bit] == '0')
                    {
                        zeros++;
                    }
                    else
                    {
                        ones++;
                    }
                }

                var filter = filterBuilder(ones, zeros);
                values.RemoveAll(x => x[bit] != filter);
                if (values.Count == 1)
                {
                    break;
                }
            }

            return values.Single();
        }
    }
}
