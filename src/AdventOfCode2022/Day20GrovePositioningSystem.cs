using AdventOfCode.Core;

namespace AdventOfCode2022;

public class Day20GrovePositioningSystem : IChallenge
{
    public int ChallengeId => 20;

    public object SolvePart1(string input)
    {
        var encrypted = File.Parse(input);
        var decrypted = encrypted.Decrypt(1);

        return decrypted.GetCoordinates();
    }

    public object SolvePart2(string input)
    {
        var encrypted = File.Parse(input);

        var decrypted = encrypted.ApplyKey();
        decrypted = decrypted.Decrypt(10);

        return decrypted.GetCoordinates();
    }

    private class File
    {
        private readonly List<Entry> _entries;

        private File(IEnumerable<Entry> contents)
        {
            _entries = contents.ToList();
        }

        public IReadOnlyList<Entry> Entries => _entries;

        public File ApplyKey()
        {
            const int decryptionKey = 811589153;

            var newEntries = new List<Entry>();
            foreach (var entry in _entries)
            {
                var newEntry = entry with { Number = entry.Number * decryptionKey };
                newEntries.Add(newEntry);
            }

            return new File(newEntries);
        }

        public File Decrypt(long numberOfPasses)
        {
            var decrypted = Entries.ToList();
            for (var pass = 0; pass < numberOfPasses; pass++)
            {
                foreach (var entry in Entries)
                {
                    if (entry.Number == 0)
                    {
                        continue;
                    }

                    var originalIndex = decrypted.IndexOf(entry);
                    decrypted.RemoveAt(originalIndex);

                    var newIndex = GetNewIndex(originalIndex, entry.Number);

                    decrypted.Insert((int)newIndex, entry);
                }
            }

            return new File(decrypted);
        }

        private long GetNewIndex(long originalIndex, long offset)
        {
            var newIndex = (originalIndex + offset) % (Entries.Count - 1);
            if (newIndex < 0)
            {
                return newIndex + Entries.Count - 1;
            }

            return newIndex;
        }

        public long GetCoordinates()
        {
            var markerIndex = _entries.FindIndex(x => x.Number == 0);
            var firstCoordinate = Entries[(markerIndex + 1000) % Entries.Count].Number;
            var secondCoordinate = Entries[(markerIndex + 2000) % Entries.Count].Number;
            var thirdCoordinate = Entries[(markerIndex + 3000) % Entries.Count].Number;

            return firstCoordinate + secondCoordinate + thirdCoordinate;
        }

        public static File Parse(string input)
        {
            var contents = input
                .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select((value, index) => new Entry(int.Parse(value), index));

            return new File(contents);
        }
    }

    private record Entry(long Number, long Index);
}