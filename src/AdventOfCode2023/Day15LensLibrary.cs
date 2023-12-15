using System.Text.RegularExpressions;
using AdventOfCode.Core;

namespace AdventOfCode2023;

public partial class Day15LensLibrary : IChallenge
{
    public int ChallengeId => 15;

    public object SolvePart1(string input) => input
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Select(GetHash)
        .Sum();

    public object SolvePart2(string input)
    {
        var boxes = ArrangeLenses(input);
        return GetFocalStrength(boxes);
    }

    private static long GetHash(string input)
    {
        var hash = 0;
        foreach (var character in input.Trim())
        {
            hash += character;
            hash *= 17;
            hash %= 256;
        }

        return hash;
    }

    private static List<Lens>[] ArrangeLenses(string input)
    {
        var instructions = input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var boxes = Enumerable.Range(0, 256).Select(_ => new List<Lens>()).ToArray();

        var regex = InstructionLabel();
        foreach (var instruction in instructions)
        {
            var match = regex.Match(instruction);
            var label = match.Groups["label"].Value;
            var operation = match.Groups["op"].Value;

            var box = boxes[GetHash(label)];
            if (operation == "-")
            {
                box.RemoveAll(x => x.Label == label);
            }
            else
            {
                var focalLength = int.Parse(match.Groups["focalLength"].Value);
                var lens = new Lens(label, focalLength);

                var index = box.FindIndex(x => x.Label == lens.Label);
                if (index >= 0)
                {
                    box[index] = lens;
                }
                else
                {
                    box.Add(lens);
                }
            }
        }

        return boxes;
    }

    private static object GetFocalStrength(IReadOnlyList<List<Lens>> boxes)
    {
        var power = 0;
        for (var index = 0; index < boxes.Count; index++)
        {
            var box = boxes[index];
            for (var slot = 0; slot < box.Count; slot++)
            {
                var boxNumber = index + 1;
                var slotNumber = slot + 1;
                var focalLength = box[slot].FocalLength;
                power += boxNumber * slotNumber * focalLength;
            }
        }

        return power;
    }

    [GeneratedRegex(@"^(?<label>\w+)(?<op>-|=(?<focalLength>\d+))$")]
    private static partial Regex InstructionLabel();

    private record Lens(string Label, int FocalLength)
    {
        public override string ToString() => $"[{Label} {FocalLength}]";
    }
}
