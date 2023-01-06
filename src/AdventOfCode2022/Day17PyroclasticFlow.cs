using System.Collections;
using AdventOfCode.Core;
using AdventOfCode.Core.Extensions;

namespace AdventOfCode2022;

public class Day17PyroclasticFlow : IChallenge
{
    private readonly RockFormation[] _rockFormations =
    {
        RockFormation.Type1, RockFormation.Type2, RockFormation.Type3, RockFormation.Type4, RockFormation.Type5
    };

    public int ChallengeId => 17;

    public object SolvePart1(string input)
    {
        const int numberOfRocks = 2022;
        return RunRockFallingSimulation(input, numberOfRocks);
    }

    public object SolvePart2(string input)
    {
        const long numberOfRocks = 1000000000000;
        return RunRockFallingSimulation(input, numberOfRocks);
    }

    private long RunRockFallingSimulation(string input, long numberOfRocks)
    {
        var maxY = -1L;
        var movementIndex = 0;
        var settledRocks = new HashSet<Point>();
        var movements = ParseMovements(input);
        var maxFormationHeight = _rockFormations.Sum(x => x.Height);

        for (var rockIndex = 0L; rockIndex < numberOfRocks; rockIndex++)
        {
            var formation = _rockFormations[rockIndex % _rockFormations.Length];

            var rock = new Rock(formation, new Point(2, maxY + 4));

            do
            {
                var movement = movements[movementIndex % movements.Count];
                rock.TryMove(movement, settledRocks);
                movementIndex++;
            } while (rock.TryMove(Movement.Down, settledRocks));

            settledRocks.UnionWith(rock);
            maxY = long.Max(maxY, rock.Max(x => x.Y));

            settledRocks.RemoveWhere(point => point.Y < maxY - maxFormationHeight);
        }

        // add 1 since 0 is counted as the first row of rock
        return maxY + 1;
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

    private record RockFormation
    {
        public static readonly RockFormation Type1 = new("1", new Boundary[]
        {
            new(0, 0), new(1, 0), new(2, 0), new(3, 0)
        });

        public static readonly RockFormation Type2 = new("2", new Boundary[]
        {
            new(1, 0), new(0, 1), new(1, 1), new(2, 1), new(1, 2)
        });

        public static readonly RockFormation Type3 = new("3", new Boundary[]
        {
            new(0, 0), new(1, 0), new(2, 0), new(2, 1), new(2, 2)
        });

        public static readonly RockFormation Type4 = new("4", new Boundary[]
        {
            new(0, 0), new(0, 1), new(0, 2), new(0, 3)
        });

        public static readonly RockFormation Type5 = new("5", new Boundary[]
        {
            new(0, 0), new(0, 1), new(1, 0), new(1, 1)
        });

        public RockFormation(string id, IReadOnlyCollection<Boundary> boundaries)
        {
            Id = id;
            Boundaries = boundaries;
        }

        public string Id { get; }
        public IReadOnlyCollection<Boundary> Boundaries { get; }
        public int Width => CalculateBoundaryDistance(x => x.XOffset);
        public int Height => CalculateBoundaryDistance(x => x.YOffset);

        private int CalculateBoundaryDistance(Func<Boundary, int> selector)
        {
            var range = Boundaries.ToRange(selector)!;
            return range.End - range.Start + 1;
        }
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
}