using System.Text.RegularExpressions;
using AdventOfCode.Core;

namespace AdventOfCode2022;

public class Day11MonkeyInTheMiddle : IChallenge
{
    public int ChallengeId => 11;

    public object SolvePart1(string input)
    {
        const int numberOfRounds = 20;
        var monkeys = ParseMonkeys(input).ToList();
        return PlayGame(monkeys, numberOfRounds, worry => worry / 3);
    }

    public object SolvePart2(string input)
    {
        const int numberOfRounds = 10000;
        var monkeys = ParseMonkeys(input).ToList();
        var leastCommonMultiple = LeastCommonMultiple(monkeys);
        return PlayGame(monkeys, numberOfRounds, worry => worry % leastCommonMultiple);
    }

    private static object PlayGame(List<Monkey> monkeys, int numberOfRounds, Func<long, long> relieve)
    {
        for (var round = 0; round < numberOfRounds; round++)
        {
            foreach (var monkey in monkeys)
            {
                while (monkey.Items.Any())
                {
                    var itemWorry = monkey.Inspect();
                    itemWorry = relieve(itemWorry);
                    monkey.Throw(itemWorry, monkeys);
                }
            }
        }

        return monkeys
            .OrderByDescending(x => x.Inspections)
            .Take(2)
            .Aggregate(1L, (current, monkey) => current * monkey.Inspections);
    }

    private static IEnumerable<Monkey> ParseMonkeys(string input)
    {
        var lines = input.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        for (var index = 0; index < lines.Length; index += 6)
        {
            yield return Monkey.Parse(lines[index..(index + 6)]);
        }
    }

    private static long GreatestCommonDenominator(long left, long right)
    {
        if (right == 0)
        {
            return left;
        }

        return GreatestCommonDenominator(right, left % right);
    }

    private static long LeastCommonMultiple(List<Monkey> monkeys)
    {
        var numbers = monkeys.Select(x => x.Test.Denominator).Order();
        return numbers.Aggregate((left, val) => left * val / GreatestCommonDenominator(left, val));
    }

    private class Monkey
    {
        private readonly Queue<long> _items;
        private readonly Operation _operation;

        public Monkey(long id, IEnumerable<long> items, Operation operation, Test test)
        {
            Id = id;
            _items = new Queue<long>(items);
            _operation = operation;
            Test = test;
        }

        public long Id { get; }
        public Test Test { get; }

        public IReadOnlyCollection<long> Items => _items;
        public long Inspections { get; private set; }

        public long Inspect()
        {
            var itemWorry = _items.Dequeue();
            itemWorry = _operation.CalculateWorry(itemWorry);
            Inspections++;

            return itemWorry;
        }

        public void Throw(long item, IEnumerable<Monkey> monkeys)
        {
            var newMonkeyId = Test.TestWorryLevel(item);
            var newMonkey = monkeys.Single(x => x.Id == newMonkeyId);
            newMonkey._items.Enqueue(item);
        }

        public static Monkey Parse(string[] lines)
        {
            var idMatch = Regex.Match(lines[0], @"Monkey (?<id>\d+):");
            var itemMatches = Regex.Matches(lines[1], @"\d+");

            var id = long.Parse(idMatch.Groups["id"].Value);
            var items = new Queue<long>(itemMatches.Select(x => long.Parse(x.Value)));
            var operation = Operation.Parse(lines[2]);
            var test = Test.Parse(lines[3..]);

            return new Monkey(id, items, operation, test);
        }

        public override string ToString()
        {
            var itemList = string.Join(", ", _items);
            return $"#{Id} ({itemList})";
        }
    }

    private class Test
    {
        public Test(long denominator, long onTrue, long onFalse)
        {
            Denominator = denominator;
            OnTrue = onTrue;
            OnFalse = onFalse;
        }

        public long Denominator { get; }
        public long OnTrue { get; }
        public long OnFalse { get; }

        public long TestWorryLevel(long worryLevel) => worryLevel % Denominator == 0 ? OnTrue : OnFalse;

        public static Test Parse(string[] lines)
        {
            var denominatorMatch = Regex.Match(lines[0], @"divisible by (?<denominator>\d+)");
            var onTrueMatch = Regex.Match(lines[1], @"If true: throw to monkey (?<onTrue>\d+)");
            var onFalseMatch = Regex.Match(lines[2], @"If false: throw to monkey (?<onFalse>\d+)");

            var denominator = int.Parse(denominatorMatch.Groups["denominator"].Value);
            var onTrue = int.Parse(onTrueMatch.Groups["onTrue"].Value);
            var onFalse = int.Parse(onFalseMatch.Groups["onFalse"].Value);
            return new Test(denominator, onTrue, onFalse);
        }

        public override string ToString() => $"if divisible by {Denominator} then {OnTrue} else {OnFalse}";
    }

    private enum Operator
    {
        Add,
        Multiply
    }

    public interface ISymbol
    {
    }

    public class Old : ISymbol
    {
        public override string ToString() => "old";
    }

    public class Constant : ISymbol
    {
        public Constant(int value)
        {
            Value = value;
        }

        public int Value { get; }

        public override string ToString() => Value.ToString();
    }

    private class Operation
    {
        public Operation(ISymbol left, Operator op, ISymbol right)
        {
            Left = left;
            Operator = op;
            Right = right;
        }

        public ISymbol Left { get; }
        public Operator Operator { get; }
        public ISymbol Right { get; }

        public long CalculateWorry(long worryLevel)
        {
            var left = EvaluateSymbol(Left, worryLevel);
            var right = EvaluateSymbol(Right, worryLevel);

            return Operator switch
            {
                Operator.Add => left + right,
                Operator.Multiply => left * right,
                _ => throw new ArgumentException()
            };
        }

        private static long EvaluateSymbol(ISymbol symbol, long worryLevel)
        {
            return symbol switch
            {
                Old => worryLevel,
                Constant constant => constant.Value,
                _ => throw new ArgumentException()
            };
        }

        public static Operation Parse(string line)
        {
            var match = Regex.Match(line, @"Operation: new = (?<left>old|\d+) (?<operator>\*|\+) (?<right>old|\d+)");
            ISymbol left = match.Groups["left"].Value == "old" ? new Old() : new Constant(int.Parse(match.Groups["left"].Value));
            ISymbol right = match.Groups["right"].Value == "old" ? new Old() : new Constant(int.Parse(match.Groups["right"].Value));
            var op = match.Groups["operator"].Value switch
            {
                "+" => Operator.Add,
                "*" => Operator.Multiply,
                _ => throw new ArgumentException()
            };

            return new Operation(left, op, right);
        }

        public override string ToString()
        {
            var op = Operator switch
            {
                Operator.Add => "+",
                Operator.Multiply => "*",
                _ => throw new ArgumentException()
            };

            return $"new = {Left} {op} {Right}";
        }
    }
}