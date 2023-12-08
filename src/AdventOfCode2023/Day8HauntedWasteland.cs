using System.Text.RegularExpressions;
using AdventOfCode.Core;
using AdventOfCode.Core.Extensions;

namespace AdventOfCode2023;

public partial class Day8HauntedWasteland : IChallenge
{
    public int ChallengeId => 8;

    public object SolvePart1(string input)
    {
        var lines = input.GetLines();
        var instructions = lines[0];
        var map = ParseNodes(lines[1..]);

        return FindPath("AAA", instructions, map);
    }

    public object SolvePart2(string input)
    {
        var lines = input.GetLines();
        var instructions = lines[0];
        var map = ParseNodes(lines[1..]);

        var ghostPaths = map
            .Keys
            .Where(x => x.EndsWith("A"))
            .Select(position => FindPath(position, instructions, map));

        return ExtraMath.LeastCommonMultiple(ghostPaths);
    }

    private static long FindPath(string position, string instructions, IReadOnlyDictionary<string, Node> map)
    {
        var steps = 0L;
        while (!position.EndsWith("Z"))
        {
            var directionToMove = instructions[(int)steps % instructions.Length];
            position = directionToMove switch
            {
                'L' => map[position].Left,
                'R' => map[position].Right,
                _ => throw new ArgumentException()
            };

            steps++;
        }

        return steps;
    }

    private static Dictionary<string, Node> ParseNodes(IEnumerable<string> lines)
    {
        var regex = NodeRegex();

        return lines
            .Select(line => regex.Match(line))
            .ToDictionary(match => match.Groups["node"].Value, match => new Node(match.Groups["left"].Value, match.Groups["right"].Value));
    }

    [GeneratedRegex(@"(?<node>\w+)\s=\s\((?<left>\w+),\s(?<right>\w+)\)", RegexOptions.Compiled)]
    private static partial Regex NodeRegex();

    private record Node(string Left, string Right);
}
