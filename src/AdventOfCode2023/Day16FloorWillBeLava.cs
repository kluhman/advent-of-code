using AdventOfCode.Core;
using AdventOfCode.Core.Extensions;

namespace AdventOfCode2023;

public class Day16FloorWillBeLava : IChallenge
{
    public int ChallengeId => 16;

    public object SolvePart1(string input)
    {
        var floor = input
            .GetLines()
            .Select(x => x.ToCharArray())
            .ToArray();

        return TrackBeam(new Beam(new Coordinates(0, 0), Direction.Right), floor);
    }

    public object SolvePart2(string input)
    {
        var floor = input
            .GetLines()
            .Select(x => x.ToCharArray())
            .ToArray();

        return Enumerable
            .Range(0, floor.Length)
            .SelectMany(y => new[]
            {
                new Beam(new Coordinates(0, y), Direction.Right),
                new Beam(new Coordinates(floor[y].Length - 1, y), Direction.Left)
            })
            .Concat(Enumerable
                .Range(0, floor[0].Length)
                .SelectMany(x => new[]
                {
                    new Beam(new Coordinates(x, 0), Direction.Down),
                    new Beam(new Coordinates(x, floor.Length - 1), Direction.Up)
                }))
            .Select(beam => TrackBeam(beam, floor))
            .Max();
    }

    private static int TrackBeam(Beam initialBeam, char[][] floor)
    {
        var beams = new Stack<Beam>();
        beams.Push(initialBeam);

        var visits = new HashSet<Beam>();
        while (beams.TryPop(out var beam))
        {
            var withinHeight = beam.Position.Y >= 0 && beam.Position.Y < floor.Length;
            var withinWidth = beam.Position.X >= 0 && beam.Position.X < floor[0].Length;
            if (!withinWidth || !withinHeight)
            {
                continue;
            }

            if (!visits.Add(beam))
            {
                continue;
            }

            var newDirection = default(Direction?);
            var tile = floor[beam.Position.Y][beam.Position.X];

            switch (tile)
            {
                case '.':
                    newDirection = beam.Direction;
                    break;
                case '-' when beam.Direction is Direction.Left or Direction.Right:
                    newDirection = beam.Direction;
                    break;
                case '-' when beam.Direction is Direction.Up or Direction.Down:
                    beams.Push(beam.Move(Direction.Left));
                    beams.Push(beam.Move(Direction.Right));
                    break;
                case '|' when beam.Direction is Direction.Up or Direction.Down:
                    newDirection = beam.Direction;
                    break;
                case '|' when beam.Direction is Direction.Right or Direction.Left:
                    beams.Push(beam.Move(Direction.Up));
                    beams.Push(beam.Move(Direction.Down));
                    break;
                case '/' when beam.Direction is Direction.Right:
                    newDirection = Direction.Up;
                    break;
                case '/' when beam.Direction is Direction.Left:
                    newDirection = Direction.Down;
                    break;
                case '/' when beam.Direction is Direction.Up:
                    newDirection = Direction.Right;
                    break;
                case '/' when beam.Direction is Direction.Down:
                    newDirection = Direction.Left;
                    break;
                case '\\' when beam.Direction is Direction.Right:
                    newDirection = Direction.Down;
                    break;
                case '\\' when beam.Direction is Direction.Left:
                    newDirection = Direction.Up;
                    break;
                case '\\' when beam.Direction is Direction.Up:
                    newDirection = Direction.Left;
                    break;
                case '\\' when beam.Direction is Direction.Down:
                    newDirection = Direction.Right;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tile));
            }

            if (!newDirection.HasValue)
            {
                continue;
            }

            beams.Push(beam.Move(newDirection.Value));
        }

        return visits.Select(x => x.Position).Distinct().Count();
    }

    private record Beam(Coordinates Position, Direction Direction)
    {
        public Beam Move(Direction direction) => new(Position + direction, direction);
    }

    private record Coordinates(int X, int Y)
    {
        public static Coordinates operator +(Coordinates coordinates, Direction direction) => direction switch
        {
            Direction.Left => coordinates with { X = coordinates.X - 1 },
            Direction.Right => coordinates with { X = coordinates.X + 1 },
            Direction.Up => coordinates with { Y = coordinates.Y - 1 },
            Direction.Down => coordinates with { Y = coordinates.Y + 1 },
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }

    private enum Direction
    {
        Left,
        Right,
        Up,
        Down
    }
}
