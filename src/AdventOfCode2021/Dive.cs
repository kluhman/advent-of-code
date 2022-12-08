using AdventOfCode.Core;

namespace AdventOfCode2021
{
    internal class Dive : IChallenge
    {
        public int ChallengeId => 2;

        public object SolvePart1(string input)
        {
            var depth = 0;
            var horizontal = 0;
            var lines = input.Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var line in lines)
            {
                var commandParameters = line.Split(' ', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                var direction = commandParameters[0];
                var units = int.Parse(commandParameters[1]);

                switch(direction)
                {
                    case "forward": 
                        horizontal += units;
                        break;
                    case "up":
                        depth -= units;
                        break;
                    case "down":
                        depth += units;
                        break;
                }
            }

            return horizontal * depth;
        }

        public object SolvePart2(string input)
        {
            var aim = 0;
            var depth = 0;
            var horizontal = 0;
            var lines = input.Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var line in lines)
            {
                var commandParameters = line.Split(' ', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                var direction = commandParameters[0];
                var units = int.Parse(commandParameters[1]);

                switch (direction)
                {
                    case "forward":
                        horizontal += units;
                        depth += aim * units;
                        break;
                    case "up":
                        aim -= units;
                        break;
                    case "down":
                        aim += units;
                        break;
                }
            }

            return horizontal * depth;
        }
    }
}
