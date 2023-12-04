using System.Collections.Immutable;
using AdventOfCode.Core;
using AdventOfCode.Core.Extensions;

namespace AdventOfCode2023;

public class Day3GearRatios : IChallenge
{
    public int ChallengeId => 3;

    public object SolvePart1(string input)
    {
        var map = Map.Parse(input);
        var partNumbers = map
            .Numbers
            .Where(number => map.Symbols.Any(number.IsAdjacent))
            .ToImmutableArray();

        return partNumbers.Sum(x => x.Value);
    }

    public object SolvePart2(string input)
    {
        var map = Map.Parse(input);

        var sum = 0;
        foreach (var symbol in map.Symbols)
        {
            if (symbol.Value != '*')
            {
                continue;
            }

            var numbers = map.Numbers.Where(number => number.IsAdjacent(symbol)).ToImmutableArray();
            if (numbers.Length != 2)
            {
                continue;
            }

            sum += numbers[0].Value * numbers[1].Value;
        }

        return sum;
    }

    private record Point(int X, int Y, char Value)
    {
        public bool IsAdjacent(Point point)
        {
            var xDiff = Math.Abs(point.X - X);
            var yDiff = Math.Abs(point.Y - Y);
            return xDiff <= 1 && yDiff <= 1;
        }
    }

    private record Number
    {
        public Number(IEnumerable<Point> points)
        {
            Points = points.ToImmutableArray();
            Value = int.Parse(new string(Points.Select(x => x.Value).ToArray()));
        }

        public int Value { get; }
        public IReadOnlyCollection<Point> Points { get; }

        public bool IsAdjacent(Point point) => Points.Any(x => x.IsAdjacent(point));
    }

    private class Map
    {
        public Map(IReadOnlyCollection<Point> symbols, IReadOnlyCollection<Number> numbers)
        {
            Symbols = symbols;
            Numbers = numbers;
        }

        public IReadOnlyCollection<Point> Symbols { get; }
        public IReadOnlyCollection<Number> Numbers { get; }

        public static Map Parse(string input)
        {
            var symbols = new List<Point>();
            var numbers = new List<Number>();
            var currentNumber = new List<Point>();

            var lines = input.GetLines();
            for (var y = 0; y < lines.Length; y++)
            {
                var line = lines[y];
                for (var x = 0; x < line.Length; x++)
                {
                    var point = new Point(x, y, line[x]);
                    if (char.IsDigit(line[x]))
                    {
                        currentNumber.Add(point);
                        continue;
                    }

                    AddNumber();

                    if (line[x] == '.')
                    {
                        continue;
                    }

                    symbols.Add(new Point(x, y, line[x]));
                }

                AddNumber();
            }

            return new Map(symbols, numbers);

            void AddNumber()
            {
                if (currentNumber.Any())
                {
                    numbers.Add(new Number(currentNumber));
                    currentNumber.Clear();
                }
            }
        }
    }
}
