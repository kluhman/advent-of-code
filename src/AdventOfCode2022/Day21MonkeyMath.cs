using System.Text.RegularExpressions;
using AdventOfCode.Core;

namespace AdventOfCode2022;

public class Day21MonkeyMath : IChallenge
{
    public int ChallengeId => 21;

    public object SolvePart1(string input)
    {
        var monkeys = ParseMonkeys(input);
        monkeys["root"].Expression.TryEvaluate(monkeys, out var result);
        return result;
    }

    public object SolvePart2(string input)
    {
        var monkeys = ParseMonkeys(input);
        var root = MakeRootEqualityCheck(monkeys);
        MakeHumanVariable(monkeys);

        return SolveForVariable(0, root.Expression, monkeys);
    }

    private static Dictionary<string, Monkey> ParseMonkeys(string input) => input
        .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
        .Select(Monkey.Parse)
        .ToDictionary(x => x.Id, x => x);

    private static Monkey MakeRootEqualityCheck(IDictionary<string, Monkey> monkeys)
    {
        var root = monkeys["root"];
        var originalExpression = (OperationExpression)root.Expression;

        root = new Monkey(root.Id, new OperationExpression(originalExpression.Left, Operator.CheckEquality, originalExpression.Right));
        monkeys[root.Id] = root;

        return root;
    }

    private static void MakeHumanVariable(IDictionary<string, Monkey> monkeys)
    {
        var human = monkeys["humn"];
        human = new Monkey(human.Id, new VariableExpression());
        monkeys[human.Id] = human;
    }

    private static long SolveForVariable(long valueNeeded, Expression equation, IReadOnlyDictionary<string, Monkey> monkeys)
    {
        if (equation is not OperationExpression operation)
        {
            return valueNeeded;
        }

        // check if we know the left side of the equation
        if (operation.Left.TryEvaluate(monkeys, out var left))
        {
            valueNeeded = ReverseOperation(valueNeeded, left, true, operation.Op);
            return SolveForVariable(valueNeeded, operation.Right.Reduce(monkeys), monkeys);
        }

        // if we don't know the left, then we know the right since there is only one variable in this problem
        operation.Right.TryEvaluate(monkeys, out var right);

        valueNeeded = ReverseOperation(valueNeeded, right, false, operation.Op);
        return SolveForVariable(valueNeeded, operation.Left.Reduce(monkeys), monkeys);
    }

    private static long ReverseOperation(long valueNeeded, long knownValue, bool isLeft, Operator op) => op switch
    {
        Operator.CheckEquality => knownValue,
        Operator.Add => valueNeeded - knownValue,
        Operator.Multiply => valueNeeded / knownValue,

        // need to subtract or add based on whether we know the left or right value
        Operator.Subtract when isLeft => knownValue - valueNeeded,
        Operator.Subtract => valueNeeded + knownValue,

        // need to divide or add based on whether we know the left or right value
        Operator.Divide when isLeft => knownValue / valueNeeded,
        Operator.Divide => valueNeeded * knownValue,
        _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
    };

    private enum Operator
    {
        Add,
        Multiply,
        Subtract,
        Divide,
        CheckEquality
    }

    private class Monkey
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

    private abstract class Expression
    {
        public abstract bool TryEvaluate(IReadOnlyDictionary<string, Monkey> monkeys, out long result);

        public virtual Expression Reduce(IReadOnlyDictionary<string, Monkey> monkeys) => this;

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

    private class NumberExpression : Expression
    {
        public NumberExpression(long number)
        {
            Number = number;
        }

        public long Number { get; }

        public override bool TryEvaluate(IReadOnlyDictionary<string, Monkey> monkeys, out long result)
        {
            result = Number;
            return true;
        }
    }

    private class OperationExpression : Expression
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

        public override bool TryEvaluate(IReadOnlyDictionary<string, Monkey> monkeys, out long result)
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

    private class MonkeyExpression : Expression
    {
        public MonkeyExpression(string monkeyId)
        {
            MonkeyId = monkeyId;
        }

        public string MonkeyId { get; }

        public override bool TryEvaluate(IReadOnlyDictionary<string, Monkey> monkeys, out long result) =>
            Reduce(monkeys).TryEvaluate(monkeys, out result);

        public override Expression Reduce(IReadOnlyDictionary<string, Monkey> monkeys) =>
            monkeys[MonkeyId].Expression;
    }

    private class VariableExpression : Expression
    {
        public override bool TryEvaluate(IReadOnlyDictionary<string, Monkey> monkeys, out long result)
        {
            result = 0;
            return false;
        }
    }
}