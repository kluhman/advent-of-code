﻿using AdventOfCode.Core;
using AdventOfCode.Core.Extensions;

namespace AdventOfCode2022;

internal class Day9RopeBridge : IChallenge
{
    public int ChallengeId => 9;

    public object SolvePart1(string input)
    {
        const int numberOfKnots = 2;
        return CalculateUniqueTailPositions(input, numberOfKnots);
    }

    public object SolvePart2(string input)
    {
        const int numberOfKnots = 10;
        return CalculateUniqueTailPositions(input, numberOfKnots);
    }

    private static int CalculateUniqueTailPositions(string input, int numberOfKnots)
    {
        var head = new Head(numberOfKnots);
        foreach (var move in ParseMoves(input))
        {
            head.Move(move);
        }

        return head.GetEnd()!.UniquePositions;
    }

    private static IEnumerable<Move> ParseMoves(string input)
    {
        var lines = input.GetLines();
        foreach (var line in lines)
        {
            var parts = line.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var units = int.Parse(parts[1]);

            for (var i = 0; i < units; i++)
            {
                yield return parts[0] switch
                {
                    "R" => Move.Right,
                    "L" => Move.Left,
                    "U" => Move.Up,
                    "D" => Move.Down,
                    _ => throw new ArgumentException()
                };
            }
        }
    }

    private record Move(int X, int Y)
    {
        public static readonly Move Left = new(-1, 0);
        public static readonly Move Right = new(1, 0);
        public static readonly Move Up = new(0, 1);
        public static readonly Move Down = new(0, -1);
    }

    private abstract class Knot
    {
        private readonly HashSet<(int x, int y)> _visits = new() { (0, 0) };

        protected Knot(int numberOfKnots)
        {
            numberOfKnots--;
            if (numberOfKnots >= 1)
            {
                Tail = new Tail(this, numberOfKnots);
            }
        }

        public int X { get; private set; }
        public int Y { get; private set; }
        public Tail? Tail { get; }

        public int UniquePositions => _visits.Count;

        public void Move(Move move)
        {
            X += move.X;
            Y += move.Y;

            _visits.Add((X, Y));
            Tail?.Follow();
        }
    }

    private class Head : Knot
    {
        public Head(int numberOfKnots) : base(numberOfKnots)
        {
        }

        public Tail? GetEnd()
        {
            var current = Tail;
            while (current?.Tail is not null)
            {
                current = current.Tail;
            }

            return current;
        }
    }

    private class Tail : Knot
    {
        public Tail(Knot head, int numberOfKnots) : base(numberOfKnots)
        {
            Head = head;
        }

        public Knot Head { get; }

        public void Follow()
        {
            var xDifference = Head.X - X;
            var yDifference = Head.Y - Y;
            var absoluteXDifference = Math.Abs(xDifference);
            var absoluteYDifference = Math.Abs(yDifference);

            if (absoluteXDifference <= 1 && absoluteYDifference <= 1)
            {
                return;
            }

            var x = xDifference switch
            {
                > 1 => 1,
                < -1 => -1,
                _ => xDifference
            };

            var y = yDifference switch
            {
                > 1 => 1,
                < -1 => -1,
                _ => yDifference
            };

            Move(new Move(x, y));
        }
    }
}
