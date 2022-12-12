using System.Text.RegularExpressions;
using AdventOfCode.Core;

namespace AdventOfCode2022;

public class MonkeyInTheMiddle : IChallenge
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

    private static object PlayGame(List<Monkey> monkeys, int numberOfRounds, Func<Int128, Int128> relieve)
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
            .Aggregate(Int128.One, (current, monkey) => current * monkey.Inspections);
    }

    private static IEnumerable<Monkey> ParseMonkeys(string input)
    {
        var lines = input.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        for (var index = 0; index < lines.Length; index += 6)
        {
            yield return Monkey.Parse(lines[index..(index + 6)]);
        }
    }

    private static Int128 GreatestCommonDenominator(Int128 left, Int128 right)
    {
        if (right == 0)
        {
            return left;
        }

        return GreatestCommonDenominator(right, left % right);
    }

    private static Int128 LeastCommonMultiple(List<Monkey> monkeys)
    {
        var numbers = monkeys.Select(x => x.Test.Denominator).Order();
        return numbers.Aggregate((left, val) => left * val / GreatestCommonDenominator(left, val));
    }

    private class Monkey
    {
        private readonly Queue<Int128> _items;
        private readonly Operation _operation;

        public Monkey(Int128 id, IEnumerable<Int128> items, Operation operation, Test test)
        {
            Id = id;
            _items = new Queue<Int128>(items);
            _operation = operation;
            Test = test;
        }

        public Int128 Id { get; }
        public Test Test { get; }

        public IReadOnlyCollection<Int128> Items => _items;
        public Int128 Inspections { get; private set; }

        public Int128 Inspect()
        {
            var itemWorry = _items.Dequeue();
            itemWorry = _operation.CalculateWorry(itemWorry);
            Inspections++;

            return itemWorry;
        }

        public void Throw(Int128 item, IEnumerable<Monkey> monkeys)
        {
            var newMonkeyId = Test.TestWorryLevel(item);
            var newMonkey = monkeys.Single(x => x.Id == newMonkeyId);
            newMonkey._items.Enqueue(item);
        }

        public static Monkey Parse(string[] lines)
        {
            var idMatch = Regex.Match(lines[0], @"Monkey (?<id>\d+):");
            var itemMatches = Regex.Matches(lines[1], @"\d+");

            var id = Int128.Parse(idMatch.Groups["id"].Value);
            var items = new Queue<Int128>(itemMatches.Select(x => Int128.Parse(x.Value)));
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
        public Test(Int128 denominator, Int128 onTrue, Int128 onFalse)
        {
            Denominator = denominator;
            OnTrue = onTrue;
            OnFalse = onFalse;
        }

        public Int128 Denominator { get; }
        public Int128 OnTrue { get; }
        public Int128 OnFalse { get; }

        public Int128 TestWorryLevel(Int128 worryLevel) => worryLevel % Denominator == 0 ? OnTrue : OnFalse;

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

        public Int128 CalculateWorry(Int128 worryLevel)
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

        private static Int128 EvaluateSymbol(ISymbol symbol, Int128 worryLevel)
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