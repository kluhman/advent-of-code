using System.Collections.Immutable;
using AdventOfCode.Core;
using AdventOfCode.Core.Extensions;

namespace AdventOfCode2023;

public class Day7CamelCards : IChallenge
{
    public int ChallengeId => 7;

    public object SolvePart1(string input) => CalculateWinnings(input, line => Hand.Parse(line));

    public object SolvePart2(string input) => CalculateWinnings(input, line => Hand.Parse(line, true));

    private static int CalculateWinnings(string input, Func<string, Hand> parseHand) => input
        .GetLines()
        .Select(parseHand)
        .ToImmutableArray()
        .Sort()
        .Select((t, index) => t.Bid * (index + 1))
        .Sum();

    private class Hand : IComparable<Hand>
    {
        private Hand(IEnumerable<PlayingCard> playingCards, int bid)
        {
            PlayingCards = playingCards.ToImmutableArray();
            HandType = IdentifyHandType(PlayingCards);
            Bid = bid;
        }

        public IReadOnlyList<PlayingCard> PlayingCards { get; }
        public HandType HandType { get; }
        public int Bid { get; }

        public int CompareTo(Hand? other)
        {
            if (ReferenceEquals(this, other))
            {
                return 0;
            }

            if (ReferenceEquals(null, other))
            {
                return 1;
            }

            var handTypeComparison = HandType.CompareTo(other.HandType);
            if (handTypeComparison != 0)
            {
                return handTypeComparison;
            }

            for (var i = 0; i < PlayingCards.Count; i++)
            {
                var playingCardComparison = PlayingCards[i].CompareTo(other.PlayingCards[i]);
                if (playingCardComparison != 0)
                {
                    return playingCardComparison;
                }
            }

            return 0;
        }

        public static Hand Parse(string line, bool useJokers = false)
        {
            var split = line.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            var bid = int.Parse(split[1]);
            var cards = split[0]
                .Select(card => card switch
                {
                    'A' => PlayingCard.Ace,
                    'K' => PlayingCard.King,
                    'Q' => PlayingCard.Queen,
                    'J' => useJokers ? PlayingCard.Joker : PlayingCard.Jack,
                    'T' => PlayingCard.Ten,
                    '9' => PlayingCard.Nine,
                    '8' => PlayingCard.Eight,
                    '7' => PlayingCard.Seven,
                    '6' => PlayingCard.Six,
                    '5' => PlayingCard.Five,
                    '4' => PlayingCard.Four,
                    '3' => PlayingCard.Three,
                    '2' => PlayingCard.Two,
                    _ => throw new ArgumentOutOfRangeException()
                });

            return new Hand(cards, bid);
        }

        private static HandType IdentifyHandType(IReadOnlyList<PlayingCard> playingCards)
        {
            var jokers = playingCards.Count(x => x == PlayingCard.Joker);
            var groupCounts = playingCards.GroupBy(x => x).OrderBy(x => x.Count()).Select(x => x.Count()).ToImmutableArray();
            return groupCounts switch
            {
                [5] => HandType.FiveOfKind,

                [1, 4] when jokers >= 1 => HandType.FiveOfKind,
                [1, 4] => HandType.FourOfKind,

                [2, 3] when jokers >= 2 => HandType.FiveOfKind,
                [2, 3] => HandType.FullHouse,

                [1, 1, 3] when jokers >= 1 => HandType.FourOfKind,
                [1, 1, 3] => HandType.ThreeOfKind,

                [1, 2, 2] when jokers == 1 => HandType.FullHouse,
                [1, 2, 2] when jokers == 2 => HandType.FourOfKind,
                [1, 2, 2] => HandType.TwoPair,

                [1, 1, 1, 2] when jokers >= 1 => HandType.ThreeOfKind,
                [1, 1, 1, 2] => HandType.Pair,

                _ when jokers == 1 => HandType.Pair,
                _ => HandType.HighCard
            };
        }
    }

    private enum PlayingCard
    {
        Joker,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King,
        Ace
    }

    private enum HandType
    {
        HighCard,
        Pair,
        TwoPair,
        ThreeOfKind,
        FullHouse,
        FourOfKind,
        FiveOfKind
    }
}
