using AdventOfCode.Core;
using AdventOfCode.Core.Extensions;

namespace AdventOfCode2022;

public class Day10CathodeRayTube : IChallenge
{
    public int ChallengeId => 10;

    public object SolvePart1(string input)
    {
        var signalSamples = new List<int>();
        var cpu = InitializeCpu(signalSamples);

        foreach (var instruction in ParseInstructions(input))
        {
            cpu.Perform(instruction);
        }

        return signalSamples.Sum();
    }

    public object SolvePart2(string input)
    {
        var tv = new Tv();
        foreach (var instruction in ParseInstructions(input))
        {
            tv.Cpu.Perform(instruction);
        }

        Console.WriteLine();
        return 0;
    }

    private static Cpu InitializeCpu(ICollection<int> signalSamples)
    {
        var cpu = new Cpu();
        cpu.OnTick += (_, args) =>
        {
            const int baseValue = 20;
            const int increment = 40;
            if (args.CyclesCompleted < baseValue)
            {
                return;
            }

            var isBaseCycle = args.CyclesCompleted == baseValue;
            var isIncrementFromBase = (args.CyclesCompleted - baseValue) % increment == 0;
            if (isBaseCycle || isIncrementFromBase)
            {
                signalSamples.Add(args.SignalStrength);
            }
        };
        return cpu;
    }

    private static IEnumerable<Instruction> ParseInstructions(string input)
    {
        var lines = input.GetLines();
        foreach (var line in lines)
        {
            var parts = line.Split(' ');
            yield return parts[0] switch
            {
                "addx" => new AddToRegister(int.Parse(parts[1])),
                "noop" => new NoOp(),
                _ => throw new ArgumentException()
            };
        }
    }

    private class Tv
    {
        public Tv()
        {
            Cpu = new Cpu();
            Cpu.OnTick += Draw;
        }

        public Cpu Cpu { get; }

        private static void Draw(object? sender, OnTickEventArgs args)
        {
            const int screenWidth = 40;
            var spriteStart = args.RegisterValue - 1;
            var spriteEnd = args.RegisterValue + 1;
            var drawPosition = (args.CyclesCompleted - 1) % screenWidth;

            Console.Write(drawPosition >= spriteStart && drawPosition <= spriteEnd ? "#" : ".");

            if (drawPosition == screenWidth - 1)
            {
                Console.WriteLine();
            }
        }
    }

    private class Cpu
    {
        public int Cycles { get; private set; }
        public int Register { get; private set; } = 1;
        public event EventHandler<OnTickEventArgs>? OnTick;

        public void Perform(Instruction instruction)
        {
            for (var tick = 0; tick < instruction.Cycles; tick++)
            {
                Cycles++;
                OnTick?.Invoke(this, new OnTickEventArgs(Cycles, Register));
            }

            CompleteInstruction(instruction);
        }

        private void CompleteInstruction(Instruction instruction)
        {
            switch (instruction)
            {
                case AddToRegister add:
                    Register += add.Value;
                    break;
            }
        }
    }

    public abstract class Instruction
    {
        public abstract int Cycles { get; }
    }

    public class AddToRegister : Instruction
    {
        public AddToRegister(int value)
        {
            Value = value;
        }

        public override int Cycles => 2;
        public int Value { get; }
    }

    public class NoOp : Instruction
    {
        public override int Cycles => 1;
    }

    private class OnTickEventArgs : EventArgs
    {
        public OnTickEventArgs(int cyclesCompleted, int registerValue)
        {
            CyclesCompleted = cyclesCompleted;
            RegisterValue = registerValue;
        }

        public int CyclesCompleted { get; }
        public int RegisterValue { get; }
        public int SignalStrength => CyclesCompleted * RegisterValue;
    }
}
