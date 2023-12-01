using System.Collections;
using AdventOfCode.Core;
using AdventOfCode.Core.Extensions;

namespace AdventOfCode2022;

public class Day13DistressSignal : IChallenge
{
    public int ChallengeId => 13;

    public object SolvePart1(string input) => ParseDataPairs(input)
        .Select((pair, index) => new { Index = index + 1, Pair = pair })
        .Where(x => x.Pair.IsCorrupted())
        .Sum(x => x.Index);

    public object SolvePart2(string input)
    {
        var dividerPacket1 = Packet.Parse("[[2]]");
        var dividerPacket2 = Packet.Parse("[[6]]");
        var packets = ParseDataPairs(input)
            .SelectMany(x => x)
            .Concat(new[] { dividerPacket1, dividerPacket2 })
            .Order()
            .ToList();

        var index1 = packets.IndexOf(dividerPacket1) + 1;
        var index2 = packets.IndexOf(dividerPacket2) + 1;

        return index1 * index2;
    }

    private static IEnumerable<DataPair> ParseDataPairs(string input)
    {
        var lines = input.GetLines();
        for (var index = 0; index < lines.Length; index += 2)
        {
            var endIndex = index + 2;
            yield return DataPair.Parse(lines[index..endIndex]);
        }
    }

    public class DataPair : IEnumerable<Packet>
    {
        public DataPair(Packet left, Packet right)
        {
            Left = left;
            Right = right;
        }

        public Packet Left { get; }
        public Packet Right { get; }
        public IEnumerator<Packet> GetEnumerator() => EnumeratePackets().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool IsCorrupted() => Left.CompareTo(Right) > 0;

        public static DataPair Parse(string[] lines) => new(Packet.Parse(lines[0]), Packet.Parse(lines[1]));

        private IEnumerable<Packet> EnumeratePackets()
        {
            yield return Left;
            yield return Right;
        }
    }

    public class Packet : IComparable<Packet>
    {
        private readonly IReadOnlyList<object> _list;

        private Packet(IReadOnlyList<object> list)
        {
            _list = list;
        }

        public int CompareTo(Packet? other) => CompareList(_list, other!._list) switch
        {
            true => -1,
            null => 0,
            false => 1
        };

        public static Packet Parse(string line)
        {
            var list = ParseList(line);
            return new Packet(list);
        }

        private static IReadOnlyList<object> ParseList(string line)
        {
            // every line starts with [ so we can skip that character
            var startingIndex = 1;
            return ParseList(line, ref startingIndex);
        }

        private static IReadOnlyList<object> ParseList(string line, ref int index)
        {
            var list = new List<object>();
            var currentInput = string.Empty;

            void ParseInteger()
            {
                if (string.IsNullOrWhiteSpace(currentInput))
                {
                    return;
                }

                list.Add(int.Parse(currentInput));
                currentInput = string.Empty;
            }

            while (index < line.Length)
            {
                switch (line[index])
                {
                    case '[':
                        index++;
                        list.Add(ParseList(line, ref index));
                        break;
                    case ']':
                        ParseInteger();
                        index++;
                        return list;
                    case ',':
                        ParseInteger();
                        break;
                    default:
                        currentInput += line[index];
                        break;
                }

                index++;
            }

            return list;
        }

        private static bool? CompareList(IReadOnlyList<object> left, IReadOnlyList<object> right)
        {
            var length = int.Min(left.Count, right.Count);
            for (var index = 0; index < length; index++)
            {
                bool? listInOrder;
                var leftValue = left[index];
                var rightValue = right[index];
                switch (leftValue)
                {
                    case int leftInt when rightValue is int rightInt:
                        var numbersInOrder = CompareInt(leftInt, rightInt);
                        if (numbersInOrder != null)
                        {
                            return numbersInOrder.Value;
                        }

                        break;
                    case int leftInt when rightValue is IReadOnlyList<object> rightList:
                        listInOrder = CompareList(new object[] { leftInt }, rightList);
                        if (listInOrder is not null)
                        {
                            return listInOrder.Value;
                        }

                        break;
                    case IReadOnlyList<object> leftList when rightValue is int rightInt:
                        listInOrder = CompareList(leftList, new object[] { rightInt });
                        if (listInOrder is not null)
                        {
                            return listInOrder.Value;
                        }

                        break;
                    case IReadOnlyList<object> leftList when rightValue is IReadOnlyList<object> rightList:
                        listInOrder = CompareList(leftList, rightList);
                        if (listInOrder is not null)
                        {
                            return listInOrder.Value;
                        }

                        break;
                }
            }

            if (left.Count == right.Count)
            {
                return null;
            }

            return left.Count < right.Count;
        }

        private static bool? CompareInt(int left, int right)
        {
            if (left == right)
            {
                return null;
            }

            return left < right;
        }
    }
}
