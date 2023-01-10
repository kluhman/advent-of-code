using System.Text.RegularExpressions;
using AdventOfCode.Core;

namespace AdventOfCode2022;

public class Day21MonkeyMath : IChallenge
{
    public enum Operator
    {
        Add,
        Multiply,
        Subtract,
        Divide
    }

    public int ChallengeId => 21;

    public object SolvePart1(string input)
    {
        var monkeys = ParseMonkeys(input).ToList();
        var root = monkeys.Single(x => x.Id == "root");

        root.Expression.TryEvaluate(monkeys, out var result);
        return result;
    }

    public object SolvePart2(string input) => 0;

    private static IEnumerable<Monkey> ParseMonkeys(string input) => input
        .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
        .Select(Monkey.Parse);

    public class Monkey
    {
        public Monkey(string id, Expression expression)
        {
            Id = id;
            Expression = expression;
        }

        public string Id { get; }
        public Expression Expression { get; }

        public static Monkey Parse(string input)
        {
            var match = Regex.Match(input, @"(?<id>.+): (?<expression>.+)");
            return new Monkey(match.Groups["id"].Value, Expression.Parse(match.Groups["expression"].Value));
        }
    }

    public abstract class Expression
    {
        public abstract bool TryEvaluate(IReadOnlyCollection<Monkey> monkeys, out long result);

        public static Expression Parse(string input)
        {
            var numberMatch = Regex.Match(input, @"\d+");
            if (numberMatch.Success)
            {
                return new NumberExpression(long.Parse(input));
            }

            var expressionMatch = Regex.Match(input, @"(?<left>.+) (?<op>\+|\-|\*|\/) (?<right>.+)");
            var left = new MonkeyExpression(expressionMatch.Groups["left"].Value);
            var right = new MonkeyExpression(expressionMatch.Groups["right"].Value);
            var op = expressionMatch.Groups["op"].Value switch
            {
                "+" => Operator.Add,
                "*" => Operator.Multiply,
                "/" => Operator.Divide,
                "-" => Operator.Subtract,
                _ => throw new ArgumentOutOfRangeException()
            };

            return new OperationExpression(left, op, right);
        }
    }

    public class NumberExpression : Expression
    {
        public NumberExpression(long number)
        {
            Number = number;
        }

        public long Number { get; }

        public override bool TryEvaluate(IReadOnlyCollection<Monkey> monkeys, out long result)
        {
            result = Number;
            return true;
        }
    }

    public class OperationExpression : Expression
    {
        public OperationExpression(Expression left, Operator op, Expression right)
        {
            Left = left;
            Op = op;
            Right = right;
        }

        public Expression Left { get; }
        public Operator Op { get; }
        public Expression Right { get; }

        public override bool TryEvaluate(IReadOnlyCollection<Monkey> monkeys, out long result)
        {
            result = 0;
            if (!Left.TryEvaluate(monkeys, out var left) || !Right.TryEvaluate(monkeys, out var right))
            {
                return false;
            }

            result = Op switch
            {
                Operator.Add => left + right,
                Operator.Divide => left / right,
                Operator.Subtract => left - right,
                Operator.Multiply => left * right,
                _ => throw new ArgumentOutOfRangeException()
            };

            return true;
        }
    }

    public class MonkeyExpression : Expression
    {
        public MonkeyExpression(string monkeyId)
        {
            MonkeyId = monkeyId;
        }

        public string MonkeyId { get; }

        public override bool TryEvaluate(IReadOnlyCollection<Monkey> monkeys, out long result)
        {
            var monkey = monkeys.Single(x => x.Id == MonkeyId);
            return monkey.Expression.TryEvaluate(monkeys, out result);
        }
    }

    public class VariableExpression : Expression
    {
        public override bool TryEvaluate(IReadOnlyCollection<Monkey> monkeys, out long result)
        {
            result = 0;
            return false;
        }
    }
}