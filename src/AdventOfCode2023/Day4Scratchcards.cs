using System.Collections.Immutable;
using AdventOfCode.Core;
using AdventOfCode.Core.Extensions;

namespace AdventOfCode2023;

public class Day4Scratchcards : IChallenge
{
    public int ChallengeId => 4;

    public object SolvePart1(string input)
    {
        return input
            .GetLines()
            .Select(ScratchCard.Parse)
            .Sum(x => x.Matches == 0
                ? 0
                : 1 << (x.Matches - 1));
    }

    public object SolvePart2(string input)
    {
        var cards = input.GetLines().Select(ScratchCard.Parse).ToImmutableArray();

        long sum = cards.Length;
        var winnerCache = new Dictionary<int, long>();
        for (var index = 0; index < cards.Length; index++)
        {
            sum += CalculateCopies(winnerCache, cards, index);
        }

        return sum;
    }

    private static long CalculateCopies(IDictionary<int, long> winnerCache, ImmutableArray<ScratchCard> cards, int index)
    {
        if (winnerCache.TryGetValue(index, out var copies))
        {
            return copies;
        }

        var copiesWon = cards[index].Matches;
        var totalFromCopies = Enumerable
            .Range(index + 1, copiesWon)
            .Select(copyIndex => CalculateCopies(winnerCache, cards, copyIndex))
            .Sum();

        winnerCache[index] = copiesWon + totalFromCopies;
        return winnerCache[index];
    }

    private class ScratchCard
    {
        private ScratchCard(IReadOnlySet<int> winningNumbers, IEnumerable<int> presentNumbers)
        {
            Matches = presentNumbers.Count(winningNumbers.Contains);
        }

        public int Matches { get; }

        public static ScratchCard Parse(string line)
        {
            line = line.Split(':', StringSplitOptions.TrimEntries)[1];
            var splitLists = line.Split('|', StringSplitOptions.TrimEntries);

            const StringSplitOptions splitOptions = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
            var winningNumbers = splitLists[0].Split(' ', splitOptions).Select(int.Parse).ToHashSet();
            var presentNumbers = splitLists[1].Split(' ', splitOptions).Select(int.Parse).ToImmutableArray();
            return new ScratchCard(winningNumbers, presentNumbers);
        }
    }
}
