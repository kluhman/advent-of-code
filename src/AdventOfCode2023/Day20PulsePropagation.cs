using AdventOfCode.Core;
using AdventOfCode.Core.Extensions;

namespace AdventOfCode2023;

public class Day20PulsePropagation : IChallenge
{
    public int ChallengeId => 20;

    public object SolvePart1(string input)
    {
        var processor = BuildProcessor(input);

        for (var i = 0; i < 1000; i++)
        {
            processor.PushButton();
        }

        return processor.Score;
    }

    public object SolvePart2(string input) => throw new NotImplementedException();

    private static Processor BuildProcessor(string input)
    {
        var processor = new Processor();
        foreach (var line in input.GetLines())
        {
            var split = line.Split("->", StringSplitOptions.TrimEntries);
            var label = split[0];
            var outputs = split[1].Split(',', StringSplitOptions.TrimEntries);

            if (label == Broadcast.BroadcastLabel)
            {
                processor.Connect(new Broadcast(), outputs);
            }
            else
            {
                processor.Connect(label[0] switch
                {
                    '%' => new FlipFlop(label[1..]),
                    '&' => new Conjunction(label[1..]),
                    _ => throw new ArgumentOutOfRangeException()
                }, outputs);
            }
        }

        return processor;
    }

    private enum Pulse
    {
        Low,
        High
    }

    private class Processor
    {
        private readonly Button _button;
        private readonly Dictionary<Module, IReadOnlyList<string>> _connections = new();
        private readonly Queue<(Module sender, Pulse pulse, Module receiver)> _messages = new();
        private readonly Dictionary<string, Module> _modules = new();
        private readonly Dictionary<Pulse, long> _pulseCounts;

        public Processor()
        {
            _button = new Button("button");
            _pulseCounts = Enum.GetValues<Pulse>().ToDictionary(pulse => pulse, _ => 0L);

            Connect(_button, new[] { Broadcast.BroadcastLabel });
        }

        public long Score => _pulseCounts.Values.Aggregate(1L, (current, value) => current * value);

        public void PushButton()
        {
            _button.Push();

            while (_messages.TryDequeue(out var message))
            {
                message.receiver.ReceivePulse(message.sender, message.pulse);
            }
        }

        public void Connect(Module module, IReadOnlyList<string> outputs)
        {
            _modules.Add(module.Label, module);
            _connections.Add(module, outputs);
            module.OnPulse += OnModulePulse;

            ConnectConjunctions(module, outputs);
        }

        private void ConnectConjunctions(Module module, IReadOnlyList<string> outputs)
        {
            if (module is Conjunction con)
            {
                foreach (var input in _connections.Keys.Where(key => _connections[key].Contains(con.Label)))
                {
                    con.ConnectInput(input);
                }
            }

            foreach (var conjunction in outputs.Select(output => _modules.GetValueOrDefault(output)).OfType<Conjunction>())
            {
                conjunction.ConnectInput(module);
            }
        }

        private void OnModulePulse(object? sender, OnPulseEventArgs args)
        {
            var senderModule = (Module)sender!;
            var pulse = args.Pulse;

            foreach (var connection in _connections[senderModule])
            {
                _pulseCounts[pulse]++;
                Console.WriteLine($"{senderModule.Label} -{pulse}-> {connection}");

                if (!_modules.ContainsKey(connection))
                {
                    continue;
                }

                _messages.Enqueue((senderModule, pulse, _modules[connection]));
            }
        }
    }

    private abstract class Module
    {
        protected Module(string label)
        {
            Label = label;
        }

        public string Label { get; }
        public event EventHandler<OnPulseEventArgs>? OnPulse;

        public abstract void ReceivePulse(Module module, Pulse pulse);

        protected void SendPulse(Pulse pulse) => OnPulse?.Invoke(this, new OnPulseEventArgs(pulse));
    }

    private class OnPulseEventArgs : EventArgs
    {
        public OnPulseEventArgs(Pulse pulse)
        {
            Pulse = pulse;
        }

        public Pulse Pulse { get; }
    }

    private class Button : Module
    {
        public Button(string label) : base(label)
        {
        }

        public void Push() => SendPulse(Pulse.Low);

        public override void ReceivePulse(Module module, Pulse pulse)
        {
            throw new InvalidOperationException("Buttons only send pulses");
        }
    }

    private class Broadcast : Module
    {
        public const string BroadcastLabel = "broadcaster";

        public Broadcast() : base(BroadcastLabel)
        {
        }

        public override void ReceivePulse(Module module, Pulse pulse) => SendPulse(pulse);
    }

    private class FlipFlop : Module
    {
        private bool _isOn;

        public FlipFlop(string label) : base(label)
        {
        }

        public override void ReceivePulse(Module module, Pulse pulse)
        {
            if (pulse == Pulse.High)
            {
                return;
            }

            if (_isOn)
            {
                _isOn = false;
                SendPulse(Pulse.Low);
            }
            else
            {
                _isOn = true;
                SendPulse(Pulse.High);
            }
        }
    }

    private class Conjunction : Module
    {
        private readonly Dictionary<Module, Pulse> _memory = new();

        public Conjunction(string label) : base(label)
        {
        }

        public void ConnectInput(Module input)
        {
            _memory.Add(input, Pulse.Low);
        }

        public override void ReceivePulse(Module module, Pulse pulse)
        {
            _memory[module] = pulse;

            SendPulse(_memory.Values.All(x => x == Pulse.High)
                ? Pulse.Low
                : Pulse.High);
        }
    }
}
