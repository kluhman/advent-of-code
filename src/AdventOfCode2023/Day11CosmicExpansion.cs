using AdventOfCode.Core;
using AdventOfCode.Core.Extensions;

namespace AdventOfCode2023;

public class Day11CosmicExpansion : IChallenge
{
    public int ChallengeId => 11;

    public object SolvePart1(string input)
    {
        var starMap = StarMap.Parse(input.GetLines());
        return FindDistanceBetweenGalaxies(starMap, 2);
    }

    public object SolvePart2(string input)
    {
        var starMap = StarMap.Parse(input.GetLines());
        return FindDistanceBetweenGalaxies(starMap, 1_000_000);
    }

    private static long FindDistanceBetweenGalaxies(StarMap starMap, long expansionMultiplier)
    {
        var sum = 0L;
        for (var index = 0; index < starMap.Galaxies.Count - 1; index++)
        {
            for (var pairIndex = index + 1; pairIndex < starMap.Galaxies.Count; pairIndex++)
            {
                var start = starMap.Galaxies[index];
                var destination = starMap.Galaxies[pairIndex];

                var minX = int.Min(start.X, destination.X);
                var maxX = int.Max(start.X, destination.X);
                var expandedColumns = starMap.ExpandedColumns.Count(column => column > minX && column < maxX);
                var regularColumns = maxX - minX - expandedColumns;

                var minY = int.Min(start.Y, destination.Y);
                var maxY = int.Max(start.Y, destination.Y);
                var expandedRows = starMap.ExpandedRows.Count(row => row > minY && row < maxY);
                var regularRows = maxY - minY - expandedRows;

                sum += expandedColumns * expansionMultiplier + regularColumns + expandedRows * expansionMultiplier + regularRows;
            }
        }

        return sum;
    }

    private record Space(int X, int Y);

    private class StarMap
    {
        private StarMap(IReadOnlyCollection<int> expandedColumns, IReadOnlyCollection<int> expandedRows, IReadOnlyList<Space> galaxies)
        {
            Galaxies = galaxies;
            ExpandedColumns = expandedColumns;
            ExpandedRows = expandedRows;
        }

        public IReadOnlyCollection<int> ExpandedColumns { get; }
        public IReadOnlyCollection<int> ExpandedRows { get; }
        public IReadOnlyList<Space> Galaxies { get; }

        public static StarMap Parse(IReadOnlyList<string> lines)
        {
            var galaxies = FindGalaxies(lines);
            var expandedRows = FindExpandedRows(lines);
            var expandedColumns = FindExpandedColumns(lines);
            return new StarMap(expandedColumns, expandedRows, galaxies);
        }

        private static List<Space> FindGalaxies(IReadOnlyList<string> lines)
        {
            var galaxies = new List<Space>();
            for (var y = 0; y < lines.Count; y++)
            {
                var line = lines[y];
                for (var x = 0; x < line.Length; x++)
                {
                    if (line[x] == '#')
                    {
                        galaxies.Add(new Space(x, y));
                    }
                }
            }

            return galaxies;
        }

        private static IReadOnlyCollection<int> FindExpandedRows(IReadOnlyList<string> lines)
        {
            var expandedRows = new List<int>();
            for (var y = 0; y < lines.Count; y++)
            {
                if (!lines[y].Contains('#'))
                {
                    expandedRows.Add(y);
                }
            }

            return expandedRows;
        }

        private static IReadOnlyCollection<int> FindExpandedColumns(IReadOnlyList<string> lines)
        {
            var expandedColumns = new List<int>();
            for (var x = 0; x < lines[0].Length; x++)
            {
                if (lines.All(line => line[x] != '#'))
                {
                    expandedColumns.Add(x);
                }
            }

            return expandedColumns;
        }
    }
}
