using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using AdventOfCode.Core;
using AdventOfCode.Core.Extensions;

namespace AdventOfCode2023;

public class Day1Trebuchet : IChallenge
{
    private static readonly Dictionary<string, char> DigitMap = new()
    {
        { "one", '1' },
        { "two", '2' },
        { "three", '3' },
        { "four", '4' },
        { "five", '5' },
        { "six", '6' },
        { "seven", '7' },
        { "eight", '8' },
        { "nine", '9' }
    };

    public int ChallengeId => 1;

    public object SolvePart1(string input) => input
        .GetLines()
        .Select(line => line.Where(char.IsDigit).ToImmutableArray())
        .Select(line => int.Parse(new string(new[] { line.First(), line.Last() })))
        .Sum();

    public object SolvePart2(string input) => input
        .GetLines()
        .Select(line => GetDigits(line).ToImmutableArray())
        .Select(line => int.Parse(new string(new[] { line.First(), line.Last() })))
        .Sum();

    private static IEnumerable<char> GetDigits(string line)
    {
        for (var index = 0; index < line.Length; index++)
        {
            if (char.IsDigit(line[index]))
            {
                yield return line[index];
            }
            else if (TryFindDigitSpelling(line, index, out var digit))
            {
                yield return digit.Value;
            }
        }
    }

    private static bool TryFindDigitSpelling(string line, int currentPosition, [NotNullWhen(true)] out char? digit)
    {
        digit = null;
        var remainingCharacters = line.Length - currentPosition;

        foreach (var (key, value) in DigitMap)
        {
            if (remainingCharacters < key.Length)
            {
                continue;
            }

            var word = line.Substring(currentPosition, key.Length);
            if (word != key)
            {
                continue;
            }

            digit = value;
            return true;
        }

        return false;
    }
}
