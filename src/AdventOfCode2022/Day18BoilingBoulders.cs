using System.Text.RegularExpressions;
using AdventOfCode.Core;
using AdventOfCode.Core.Models;

namespace AdventOfCode2022;

public class Day18BoilingBoulders : IChallenge
{
    public int ChallengeId => 18;

    public object SolvePart1(string input) => Cube.Parse(input).GetSurfaceArea();

    public object SolvePart2(string input) => Cube.Parse(input).GetExteriorSurfaceArea();

    public record Coordinates(int X, int Y, int Z)
    {
        public IEnumerable<Coordinates> GetSides()
        {
            yield return this with { X = X - 1 };
            yield return this with { X = X + 1 };
            yield return this with { Y = Y - 1 };
            yield return this with { Y = Y + 1 };
            yield return this with { Z = Z - 1 };
            yield return this with { Z = Z + 1 };
        }
    }

    public class Cube
    {
        private readonly HashSet<Coordinates> _lava;

        private Cube(HashSet<Coordinates> lava)
        {
            _lava = lava;
        }

        public IReadOnlySet<Coordinates> LavaDroplets => _lava;

        public static Cube Parse(string input)
        {
            var matches = Regex.Matches(input, @"(?<x>\d+),(?<y>\d+),(?<z>\d+)");
            var lava = matches.Select(match =>
            {
                var x = int.Parse(match.Groups["x"].Value);
                var y = int.Parse(match.Groups["y"].Value);
                var z = int.Parse(match.Groups["z"].Value);

                return new Coordinates(x, y, z);
            }).ToHashSet();

            return new Cube(lava);
        }

        public int GetSurfaceArea() => LavaDroplets
            .SelectMany(droplet => droplet.GetSides())
            .Count(side => !LavaDroplets.Contains(side));

        public int GetExteriorSurfaceArea()
        {
            var reachableFromExterior = GetReachablePoints();
            return LavaDroplets
                .SelectMany(droplet => droplet.GetSides())
                .Count(side => reachableFromExterior.Contains(side));
        }

        private HashSet<Coordinates> GetReachablePoints()
        {
            var reachableFromExterior = new HashSet<Coordinates>();
            var (xRange, yRange, zRange) = GetBounds();
            if (xRange is null || yRange is null || zRange is null)
            {
                return reachableFromExterior;
            }

            // start with the outside boundary of the cube known to be air, then work inwards until all air pockets are visited
            var toCheck = new Queue<Coordinates>(GetOuterBoundaries(xRange, yRange, zRange));
            while (toCheck.TryDequeue(out var point))
            {
                reachableFromExterior.Add(point);
                foreach (var side in point.GetSides())
                {
                    // skip sides that are outside the bounds of the cube
                    if (!xRange.Contains(side.X) || !yRange.Contains(side.Y) || !zRange.Contains(side.Z))
                    {
                        continue;
                    }

                    // if point is a known lava droplet, that point isn't reachable
                    if (LavaDroplets.Contains(side))
                    {
                        continue;
                    }

                    // don't visit sides we have already visited
                    if (reachableFromExterior.Contains(side))
                    {
                        continue;
                    }

                    toCheck.Enqueue(side);
                }
            }

            return reachableFromExterior;
        }

        private (Range<int>? xRange, Range<int>? yRange, Range<int>? zRange) GetBounds()
        {
            var xRange = default(Range<int>);
            var yRange = default(Range<int>);
            var zRange = default(Range<int>);

            foreach (var droplet in LavaDroplets)
            {
                xRange = xRange is null
                    ? new Range<int>(droplet.X, droplet.X)
                    : new Range<int>(int.Min(droplet.X, xRange.Start), int.Max(droplet.X, xRange.End));

                yRange = yRange is null
                    ? new Range<int>(droplet.Y, droplet.Y)
                    : new Range<int>(int.Min(droplet.Y, yRange.Start), int.Max(droplet.Y, yRange.End));

                zRange = zRange is null
                    ? new Range<int>(droplet.Z, droplet.Z)
                    : new Range<int>(int.Min(droplet.Z, zRange.Start), int.Max(droplet.Z, zRange.End));
            }

            return (xRange, yRange, zRange);
        }

        private static IEnumerable<Coordinates> GetOuterBoundaries(Range<int> xRange, Range<int> yRange, Range<int> zRange) =>
            from x in Enumerable.Range(xRange.Start - 1, xRange.End - xRange.Start + 3)
            from y in Enumerable.Range(yRange.Start - 1, yRange.End - yRange.Start + 3)
            from z in Enumerable.Range(zRange.Start - 1, zRange.End - zRange.Start + 3)
            let point = new Coordinates(x, y, z)
            where !xRange.Contains(point.X) || !yRange.Contains(point.Y) || !zRange.Contains(point.Z)
            select point;
    }
}