using System.Collections.Immutable;
using System.Text.RegularExpressions;
using AdventOfCode.Core;
using AdventOfCode.Core.Extensions;
using AdventOfCode.Core.Models;

namespace AdventOfCode2022;

public class Day22MonkeyMap : IChallenge
{
    public int ChallengeId => 22;

    public object SolvePart1(string input)
    {
        var (board, movements) = ParseBoard(input);
        foreach (var (direction, steps) in movements)
        {
            board.Move(direction, steps);
        }

        return board.Score;
    }

    public object SolvePart2(string input) => throw new NotImplementedException();

    private static (Board board, IEnumerable<(Direction direction, int steps)> movements) ParseBoard(string input)
    {
        var lines = input.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var boardLines = lines[..^1];
        var directions = lines[^1];

        return (Board.Parse(boardLines), ParseMovements(directions));
    }

    private static IEnumerable<(Direction move, int steps)> ParseMovements(string directions)
    {
        var matches = Regex.Matches(directions, @"(?<steps>\d+)(?<turn>R|L)?");

        var movement = Direction.Right;
        foreach (Match match in matches)
        {
            var steps = int.Parse(match.Groups["steps"].Value);
            yield return (movement, steps);

            if (!match.Groups["turn"].Success)
            {
                continue;
            }

            var turn = match.Groups["turn"].Value;
            movement = turn switch
            {
                "L" when movement == Direction.Left => Direction.Down,
                "L" when movement == Direction.Down => Direction.Right,
                "L" when movement == Direction.Right => Direction.Up,
                "L" when movement == Direction.Up => Direction.Left,
                "R" when movement == Direction.Left => Direction.Up,
                "R" when movement == Direction.Down => Direction.Left,
                "R" when movement == Direction.Right => Direction.Down,
                "R" when movement == Direction.Up => Direction.Right,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    private record Coordinates(int X, int Y)
    {
        public static Coordinates operator +(Coordinates position, Direction direction) => direction switch
        {
            Direction.Left => position with { X = position.X - 1 },
            Direction.Right => position with { X = position.X + 1 },
            Direction.Up => position with { Y = position.Y - 1 },
            Direction.Down => position with { Y = position.Y + 1 },
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }

    private enum Direction
    {
        Right,
        Down,
        Left,
        Up
    }

    private class Board
    {
        private readonly IReadOnlySet<Coordinates> _knownSpaces;
        private readonly Dictionary<Coordinates, Direction> _recording = new();
        private readonly IReadOnlySet<Coordinates> _walls;

        public Board(IReadOnlySet<Coordinates> openSpaces, IReadOnlySet<Coordinates> walls)
        {
            _walls = walls;
            _knownSpaces = _walls.Union(openSpaces).ToImmutableHashSet();

            CurrentPosition = openSpaces
                .Where(position => position.Y == 0)
                .MinBy(position => position.X)!;
        }

        public Coordinates CurrentPosition { get; private set; }
        public long Score => 1000 * (CurrentPosition.Y + 1) + 4 * (CurrentPosition.X + 1) + (int)_recording[CurrentPosition];

        public void Move(Direction direction, int steps)
        {
            var movingOnX = direction is Direction.Left or Direction.Right;
            var movingOnY = direction is Direction.Up or Direction.Down;
            var xRange = movingOnX ? _knownSpaces.Where(x => x.Y == CurrentPosition.Y).ToRange(x => x.X) : null;
            var yRange = movingOnY ? _knownSpaces.Where(x => x.X == CurrentPosition.X).ToRange(x => x.Y) : null;

            var position = CurrentPosition;
            for (var i = 0; i < steps; i++)
            {
                position += direction;

                if (movingOnX)
                {
                    position = CheckForHorizontalLoop(position, xRange!);
                }

                if (movingOnY)
                {
                    position = CheckForVerticalLoop(position, yRange!);
                }

                if (_walls.Contains(position))
                {
                    break;
                }

                _recording[CurrentPosition] = direction;
                CurrentPosition = position;
            }

            _recording[CurrentPosition] = direction;
        }

        private static Coordinates CheckForHorizontalLoop(Coordinates position, Range<int> xRange)
        {
            // loop around left
            if (position.X < xRange.Start)
            {
                position = position with { X = xRange.End };
            }

            // loop around right
            if (position.X > xRange.End)
            {
                position = position with { X = xRange.Start };
            }

            return position;
        }

        private static Coordinates CheckForVerticalLoop(Coordinates position, Range<int> yRange)
        {
            // loop around up
            if (position.Y < yRange.Start)
            {
                position = position with { Y = yRange.End };
            }

            // loop around down
            if (position.Y > yRange.End)
            {
                position = position with { Y = yRange.Start };
            }

            return position;
        }

        public static Board Parse(IReadOnlyList<string> lines)
        {
            var walls = new HashSet<Coordinates>();
            var openSpaces = new HashSet<Coordinates>();

            for (var y = 0; y < lines.Count; y++)
            {
                for (var x = 0; x < lines[y].Length; x++)
                {
                    var character = lines[y][x];
                    switch (character)
                    {
                        case '.':
                            openSpaces.Add(new Coordinates(x, y));
                            break;
                        case '#':
                            walls.Add(new Coordinates(x, y));
                            break;
                    }
                }
            }

            return new Board(openSpaces, walls);
        }
    }
}