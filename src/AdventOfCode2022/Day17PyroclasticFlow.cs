using System.Collections;
using System.Collections.Immutable;
using AdventOfCode.Core;
using AdventOfCode.Core.Extensions;

namespace AdventOfCode2022;

public class Day17PyroclasticFlow : IChallenge
{
    private readonly PatternFinder _patternFinder = new();

    public int ChallengeId => 17;

    public object SolvePart1(string input) => SolveWithPatternFinding(input, 2022);
    public object SolvePart2(string input) => SolveWithPatternFinding(input, 1000000000000);

    private object SolveWithPatternFinding(string input, long numberOfRocks)
    {
        var movements = ParseMovements(input);

        // we must do at least (movements * rock types) rocks to ensure every rock goes through every motion
        // the first iteration of that pattern begins on the ground, and the second iteration from the top of the highest rock, so we double the rocks
        // to ensure we find a pattern that relies on the rocks before it
        var patternSampleSize = movements.Count * RockFormation.Types.Count * 2;
        var simulator = new Simulator(movements);

        if (patternSampleSize >= numberOfRocks)
        {
            simulator.Simulate(numberOfRocks);
            return simulator.MaxHeight;
        }

        simulator.Simulate(patternSampleSize);
        var pattern = _patternFinder.FindMaximumPattern(simulator.Rocks.ToImmutableArray());
        if (pattern is null)
        {
            throw new Exception("No pattern found, try larger sample");
        }

        var rocksToSimulate = numberOfRocks - patternSampleSize;
        var patternRepetitions = rocksToSimulate / pattern.RocksInPattern;
        var height = simulator.MaxHeight + pattern.GetHeight() * patternRepetitions;

        rocksToSimulate -= patternRepetitions * pattern.RocksInPattern;
        height += pattern.GetHeight(rocksToSimulate);

        return height;
    }

    private static IReadOnlyList<Movement> ParseMovements(string input) => input.Trim()
        .Select(character => character switch
        {
            '<' => Movement.Left,
            '>' => Movement.Right,
            _ => throw new ArgumentOutOfRangeException(nameof(character), character, null)
        })
        .ToList();

    private static void DrawChamber(IReadOnlyCollection<Rock> rocks)
    {
        var points = rocks.SelectMany(rock => rock.Select(point => new { point, rock.Formation.Id })).ToHashSet();
        var map = points.ToDictionary(x => x.point, x => x.Id);
        var highPoint = map.Keys.Max(point => point.Y);
        for (var y = highPoint; y >= 0; y--)
        {
            Console.Write("|");
            for (var x = 0; x < 7; x++)
            {
                Console.Write(map.GetValueOrDefault(new Point(x, y), "."));
            }

            Console.WriteLine("|");
        }
    }

    private record Boundary(int XOffset, int YOffset);

    private record Movement(int XOffset, int YOffset)
    {
        public static readonly Movement Left = new(-1, 0);
        public static readonly Movement Right = new(1, 0);
        public static readonly Movement Down = new(0, -1);
    }

    private record RockFormation(string Id, IReadOnlyCollection<Boundary> Boundaries)
    {
        private static readonly RockFormation Type1 = new("1", new Boundary[]
        {
            new(0, 0), new(1, 0), new(2, 0), new(3, 0)
        });

        private static readonly RockFormation Type2 = new("2", new Boundary[]
        {
            new(1, 0), new(0, 1), new(1, 1), new(2, 1), new(1, 2)
        });

        private static readonly RockFormation Type3 = new("3", new Boundary[]
        {
            new(0, 0), new(1, 0), new(2, 0), new(2, 1), new(2, 2)
        });

        private static readonly RockFormation Type4 = new("4", new Boundary[]
        {
            new(0, 0), new(0, 1), new(0, 2), new(0, 3)
        });

        private static readonly RockFormation Type5 = new("5", new Boundary[]
        {
            new(0, 0), new(0, 1), new(1, 0), new(1, 1)
        });

        public static readonly IReadOnlyList<RockFormation> Types = new[] { Type1, Type2, Type3, Type4, Type5 };
    }

    private record Point(long X, long Y);

    private class Rock : IEnumerable<Point>
    {
        private const int ChamberWidth = 7;

        public Rock(RockFormation formation, Point location)
        {
            Formation = formation;
            Location = location;
        }

        public RockFormation Formation { get; }
        public Point Location { get; private set; }

        public IEnumerator<Point> GetEnumerator() => EnumerateBoundaries(Location).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool TryMove(Movement movement, IReadOnlySet<Point> otherRocks)
        {
            var newLocation = new Point(Location.X + movement.XOffset, Location.Y + movement.YOffset);
            var boundaries = EnumerateBoundaries(newLocation).ToHashSet();
            if (boundaries.Any(point => point.X is < 0 or >= ChamberWidth))
            {
                return false;
            }

            if (boundaries.Any(point => point.Y < 0))
            {
                return false;
            }

            var collisions = otherRocks.Intersect(boundaries);
            if (collisions.Any())
            {
                return false;
            }

            Location = newLocation;
            return true;
        }

        private IEnumerable<Point> EnumerateBoundaries(Point location)
        {
            foreach (var boundary in Formation.Boundaries)
            {
                yield return new Point(location.X + boundary.XOffset, location.Y + boundary.YOffset);
            }
        }
    }

    private class Simulator
    {
        private readonly IReadOnlyList<Movement> _movements;

        private readonly List<Rock> _rocks;

        public Simulator(IReadOnlyList<Movement> movements)
        {
            _rocks = new List<Rock>();
            _movements = movements;
        }

        public int MovementIndex { get; private set; }
        public long MaxHeight { get; private set; } = -1;
        public IReadOnlyList<Rock> Rocks => _rocks;

        public void Simulate(long numberOfRocks)
        {
            var settledRocks = new HashSet<Point>();
            for (var i = 0L; i < numberOfRocks; i++)
            {
                var formation = RockFormation.Types[Rocks.Count % RockFormation.Types.Count];
                var rock = new Rock(formation, new Point(2, MaxHeight + 4));

                do
                {
                    var movement = _movements[MovementIndex % _movements.Count];
                    rock.TryMove(movement, settledRocks);
                    MovementIndex++;
                } while (rock.TryMove(Movement.Down, settledRocks));

                _rocks.Add(rock);
                settledRocks.UnionWith(rock);
                MaxHeight = long.Max(MaxHeight, rock.Max(x => x.Y));

                settledRocks.RemoveWhere(x => x.Y < MaxHeight - 100);
            }

            // since we're measuring from 0, add 1 to get the max height of the structure
            MaxHeight++;
        }

        public void Draw()
        {
            var points = Rocks.SelectMany(rock => rock.Select(point => new { point, rock.Formation.Id })).ToHashSet();
            var map = points.ToDictionary(x => x.point, x => x.Id);
            var highPoint = map.Keys.Max(point => point.Y);
            for (var y = highPoint; y >= 0; y--)
            {
                Console.Write("|");
                for (var x = 0; x < 7; x++)
                {
                    Console.Write(map.GetValueOrDefault(new Point(x, y), "."));
                }

                Console.WriteLine("|");
            }
        }
    }

    private class PatternFinder
    {
        public RockPattern? FindMaximumPattern(ImmutableArray<Rock> rocks)
        {
            // only need to check half of the list, since the max pattern would be repeated exactly once in the collection
            for (var topStart = rocks.Length / 2; topStart < rocks.Length; topStart++)
            {
                var top = rocks[topStart..];
                var bottomStart = topStart - top.Length;

                if (bottomStart < 0)
                {
                    continue;
                }

                var bottom = rocks[bottomStart..topStart];
                if (IsEquivalent(top, bottom))
                {
                    return new RockPattern(top);
                }
            }

            return null;
        }

        private static bool IsEquivalent(IReadOnlyList<Rock> top, IReadOnlyList<Rock> bottom)
        {
            if (top.Count != bottom.Count)
            {
                return false;
            }

            for (var i = 0; i < top.Count; i++)
            {
                if (top[i].Location.X != bottom[i].Location.X)
                {
                    return false;
                }
            }

            return true;
        }
    }

    private class RockPattern
    {
        private readonly IReadOnlyList<Rock> _rocks;

        public RockPattern(IReadOnlyList<Rock> rocks)
        {
            _rocks = rocks;
        }

        public int RocksInPattern => _rocks.Count;

        public long GetHeight(long? numberOfRocks = null)
        {
            if (numberOfRocks is not null && numberOfRocks > RocksInPattern)
            {
                throw new ArgumentException("Cannot find height for rocks greater than number of rocks in pattern");
            }

            var rocks = numberOfRocks is null ? _rocks : _rocks.Take((int)numberOfRocks.Value);
            return GetHeight(rocks);
        }

        private static long GetHeight(IEnumerable<Rock> rocks)
        {
            var verticalRange = rocks.SelectMany(rock => rock).ToRange(x => x.Y)!;
            if (verticalRange is null)
            {
                return 0;
            }

            return verticalRange.End - verticalRange.Start + 1;
        }
    }
}