using System.Text.RegularExpressions;
using AdventOfCode.Core;

namespace AdventOfCode2022;

internal class SupplyStacks : IChallenge
{
    public int ChallengeId => 5;

    public object SolvePart1(string input)
    {
        var lines = input.Split('\n').Select(x => x.TrimEnd()).ToList();
        var dividerIndex = lines.IndexOf(string.Empty);
        var crateMap = lines.Take(dividerIndex).ToArray();
        var operations = lines.Skip(dividerIndex + 1).ToArray();

        var stacks = ParseMap(crateMap);
        MoveIndividualCrates(stacks, operations);
        return SummarizeTopCrates(stacks);
    }

    public object SolvePart2(string input)
    {
        var lines = input.Split('\n').Select(x => x.TrimEnd()).ToList();
        var dividerIndex = lines.IndexOf(string.Empty);
        var crateMap = lines.Take(dividerIndex).ToArray();
        var operations = lines.Skip(dividerIndex + 1).ToArray();

        var stacks = ParseMap(crateMap);
        MoveGroupedCrates(stacks, operations);
        return SummarizeTopCrates(stacks);
    }

    private static Stack<char>[] ParseMap(string[] crateMap)
    {
        var numberOfStacks = int.Parse(crateMap[^1].Split(' ').Last());
        var stacks = Enumerable.Range(0, numberOfStacks).Select(_ => new Stack<char>()).ToArray();

        for (var mapIndex = crateMap.Length - 2; mapIndex >= 0; mapIndex--)
        {
            var stackIndex = 0;
            var map = crateMap[mapIndex];
            for (var index = 0; index < map.Length; index++)
            {
                switch (map[index])
                {
                    case ' ':
                        index += 3;
                        stackIndex++;
                        break;
                    case '[':
                        break;
                    case ']':
                        index++;
                        stackIndex++;
                        break;
                    default:
                        stacks[stackIndex].Push(map[index]);
                        break;
                }
            }
        }

        return stacks;
    }

    private static void MoveIndividualCrates(Stack<char>[] stacks, string[] operations)
    {
        foreach (var operation in operations)
        {
            var result = Regex.Match(operation, @"^move (?<count>\d+) from (?<from>\d+) to (?<to>\d+)\s*$");
            var count = int.Parse(result.Groups["count"].Value);
            var fromIndex = int.Parse(result.Groups["from"].Value) - 1;
            var toIndex = int.Parse(result.Groups["to"].Value) - 1;

            for (var i = 0; i < count; i++)
            {
                var crate = stacks[fromIndex].Pop();
                stacks[toIndex].Push(crate);
            }
        }
    }

    private static void MoveGroupedCrates(Stack<char>[] stacks, string[] operations)
    {
        foreach (var operation in operations)
        {
            var result = Regex.Match(operation, @"^move (?<count>\d+) from (?<from>\d+) to (?<to>\d+)\s*$");
            var count = int.Parse(result.Groups["count"].Value);
            var fromIndex = int.Parse(result.Groups["from"].Value) - 1;
            var toIndex = int.Parse(result.Groups["to"].Value) - 1;

            var temp = new Stack<char>();
            for (var i = 0; i < count; i++)
            {
                var crate = stacks[fromIndex].Pop();
                temp.Push(crate);
            }

            for (var i = 0; i < count; i++)
            {
                var crate = temp.Pop();
                stacks[toIndex].Push(crate);
            }
        }
    }

    private static object SummarizeTopCrates(IEnumerable<Stack<char>> stacks)
    {
        var output = string.Empty;
        foreach (var stack in stacks)
        {
            if (!stack.TryPeek(out var crate))
            {
                continue;
            }

            output += crate;
        }

        return output;
    }
}