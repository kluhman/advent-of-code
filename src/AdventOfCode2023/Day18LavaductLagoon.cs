using System.Globalization;
using System.Text.RegularExpressions;
using AdventOfCode.Core;

namespace AdventOfCode2023;

public partial class Day18LavaDuctLagoon : IChallenge
{
    public int ChallengeId => 18;

    public object SolvePart1(string input)
    {
        var regex = InstructionRegex();
        var instructions = regex.Matches(input).Select(Part1Instruction);

        var (points, perimeter) = DigTrench(instructions);
        return GetTrenchArea(points, perimeter);
    }

    public object SolvePart2(string input)
    {
        var regex = InstructionRegex();
        var instructions = regex.Matches(input).Select(Part2Instruction);

        var (points, perimeter) = DigTrench(instructions);
        return GetTrenchArea(points, perimeter);
    }

    private static long GetTrenchArea(IReadOnlyList<Point> points, long perimeter)
    {
        // shoelace formula
        // https://en.wikipedia.org/wiki/Shoelace_formula
        var area = 0L;
        for (var index = 0; index < points.Count; index++)
        {
            var (x1, y1) = points[index];
            var (x2, y2) = points[(index + 1) % points.Count];
            area += x1 * y2 - x2 * y1;
        }

        area /= 2;
        perimeter /= 2;
        return perimeter + area + 1;
    }

    private static (IReadOnlyList<Point> points, long perimeter) DigTrench(IEnumerable<Instruction> instructions)
    {
        var perimeter = 0L;
        var points = new List<Point>();
        var position = new Point(0, 0);
        foreach (var instruction in instructions)
        {
            position = instruction.Direction switch
            {
                Direction.Up => position with { Y = position.Y - instruction.Distance },
                Direction.Down => position with { Y = position.Y + instruction.Distance },
                Direction.Left => position with { X = position.X - instruction.Distance },
                Direction.Right => position with { X = position.X + instruction.Distance },
                _ => throw new ArgumentOutOfRangeException()
            };

            points.Add(position);
            perimeter += instruction.Distance;
        }

        return (points, perimeter);
    }

    private static Instruction Part1Instruction(Match match) => new(match.Groups["letter"].Value switch
    {
        "U" => Direction.Up,
        "D" => Direction.Down,
        "R" => Direction.Right,
        "L" => Direction.Left,
        _ => throw new ArgumentOutOfRangeException()
    }, int.Parse(match.Groups["number"].Value));

    private static Instruction Part2Instruction(Match match)
    {
        var hex = match.Groups["hex"].Value;
        var distance = int.Parse(hex.Substring(0, 5), NumberStyles.HexNumber);

        return new Instruction(hex.Last() switch
        {
            '3' => Direction.Up,
            '1' => Direction.Down,
            '0' => Direction.Right,
            '2' => Direction.Left,
            _ => throw new ArgumentOutOfRangeException()
        }, distance);
    }

    [GeneratedRegex(@"(?<letter>R|L|U|D)\s(?<number>\d+)\s\(#(?<hex>[0-9a-f]{6})\)")]
    private static partial Regex InstructionRegex();

    private enum Direction
    {
        Up,
        Right,
        Left,
        Down
    }

    private record Instruction(Direction Direction, int Distance);

    private record Point(long X, long Y);
}
