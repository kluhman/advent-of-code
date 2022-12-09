using System.Drawing;
using AdventOfCode.Core;

namespace AdventOfCode2022;

internal class TreetopTreeHouse : IChallenge
{
    public int ChallengeId => 8;

    public object SolvePart1(string input)
    {
        var map = TreeMap.Parse(input);

        return GitHub.Scientist.Science<int>("Check Visibility", experiment =>
        {
            experiment.Use(() => CheckVisibilityVersion1(map));
            experiment.Try(() => CheckVisibilityVersion2(map));
        });
    }

    public object SolvePart2(string input)
    {
        var map = TreeMap.Parse(input);

        var highestScore = 0;
        for (var row = 0; row < map.Rows; row++)
        {
            for (var column = 0; column < map.Columns; column++)
            {
                var startingPoint = new Point(row, column);
                var scenicScore = CalculateScenicScore(map.TreeHeight, map.Rows, map.Columns, startingPoint);
                if (scenicScore > highestScore)
                {
                    highestScore = scenicScore;
                }
            }
        }

        return highestScore;
    }

    private static int CheckVisibilityVersion1(TreeMap map)
    {
        var visibility = new bool[map.Rows, map.Columns];
        CheckVisibilityFromLeft(map.TreeHeight, map.Rows, map.Columns, visibility);
        CheckVisibilityFromRight(map.TreeHeight, map.Rows, map.Columns, visibility);
        CheckVisibilityFromTop(map.TreeHeight, map.Rows, map.Columns, visibility);
        CheckVisibilityFromBottom(map.TreeHeight, map.Rows, map.Columns, visibility);

        return CountVisibleTrees(map.Rows, map.Columns, visibility);
    }

    private static int CheckVisibilityVersion2(TreeMap map)
    {
        var count = 0;
        for (var row = 0; row < map.Rows; row++)
        {
            for (var column = 0; column < map.Columns; column++)
            {
                if (map.IsVisible(row, column))
                {
                    count++;
                }
            }
        }

        return count;
    }

    private static void CheckVisibilityFromLeft(int[,] grid, int rows, int columns, bool[,] visibility)
    {
        for (var row = 0; row < rows; row++)
        {
            var maxHeight = -1;
            for (var column = 0; column < columns; column++)
            {
                var treeHeight = grid[row, column];
                if (treeHeight > maxHeight)
                {
                    maxHeight = treeHeight;
                    visibility[row, column] = true;
                }
            }
        }
    }

    private static void CheckVisibilityFromRight(int[,] grid, int rows, int columns, bool[,] visibility)
    {
        for (var row = 0; row < rows; row++)
        {
            var maxHeight = -1;
            for (var column = columns - 1; column >= 0; column--)
            {
                var treeHeight = grid[row, column];
                if (treeHeight > maxHeight)
                {
                    maxHeight = treeHeight;
                    visibility[row, column] = true;
                }
            }
        }
    }

    private static void CheckVisibilityFromTop(int[,] grid, int rows, int columns, bool[,] visibility)
    {
        for (var column = 0; column < columns; column++)
        {
            var maxHeight = -1;
            for (var row = 0; row < rows; row++)
            {
                var treeHeight = grid[row, column];
                if (treeHeight > maxHeight)
                {
                    maxHeight = treeHeight;
                    visibility[row, column] = true;
                }
            }
        }
    }

    private static void CheckVisibilityFromBottom(int[,] grid, int rows, int columns, bool[,] visibility)
    {
        for (var column = 0; column < columns; column++)
        {
            var maxHeight = -1;
            for (var row = rows - 1; row >= 0; row--)
            {
                var treeHeight = grid[row, column];
                if (treeHeight > maxHeight)
                {
                    maxHeight = treeHeight;
                    visibility[row, column] = true;
                }
            }
        }
    }

    private static int CountVisibleTrees(int rows, int columns, bool[,] visibility)
    {
        var visibileTrees = 0;
        for (var row = 0; row < rows; row++)
        {
            for (var column = 0; column < columns; column++)
            {
                if (visibility[row, column])
                {
                    visibileTrees++;
                }
            }
        }

        return visibileTrees;
    }

    private static int CalculateScenicScore(int[,] grid, int rows, int columns, Point startingPoint)
    {
        var temp = 0;
        var treeHeight = grid[startingPoint.X, startingPoint.Y];

        for (var column = startingPoint.Y - 1; column >= 0; column--)
        {
            temp++;
            if (grid[startingPoint.X, column] >= treeHeight)
            {
                break;
            }
        }

        var visibilityToLeft = temp;
        temp = 0;

        for (var column = startingPoint.Y + 1; column < columns; column++)
        {
            temp++;
            if (grid[startingPoint.X, column] >= treeHeight)
            {
                break;
            }
        }

        var visibilityToRight = temp;
        temp = 0;

        for (var row = startingPoint.X - 1; row >= 0; row--)
        {
            temp++;
            if (grid[row, startingPoint.Y] >= treeHeight)
            {
                break;
            }
        }

        var visibilityAbove = temp;
        temp = 0;

        for (var row = startingPoint.X + 1; row < rows; row++)
        {
            temp++;
            if (grid[row, startingPoint.Y] >= treeHeight)
            {
                break;
            }
        }

        var visibilityBelow = temp;

        return visibilityToLeft * visibilityToRight * visibilityAbove * visibilityBelow;
    }

    public class TreeMap
    {
        public TreeMap(int[,] trees)
        {
            Rows = trees.GetLength(0);
            Columns = trees.GetLength(1);
            TreeHeight = trees;
        }

        public int Rows { get; }
        public int Columns { get; }
        public int[,] TreeHeight { get; }

        public static TreeMap Parse(string input)
        {
            var lines = input.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            var rows = lines.Length;
            var columns = lines[0].Length;
            var trees = new int[rows, columns];
            for (var row = 0; row < lines.Length; row++)
            {
                var line = lines[row];
                for (var column = 0; column < line.Length; column++)
                {
                    trees[row, column] = int.Parse(line[column].ToString());
                }
            }

            return new TreeMap(trees);
        }

        public bool IsVisible(int row, int column) => IsVisibleNorth(row, column) || IsVisibleSouth(row, column) ||
                                                      IsVisibleWest(row, column) || IsVisibleEast(row, column);

        private bool IsVisibleNorth(int row, int column)
        {
            var height = TreeHeight[row, column];
            for (var x = row - 1; x >= 0; x--)
            {
                if (TreeHeight[x, column] >= height)
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsVisibleSouth(int row, int column)
        {
            var height = TreeHeight[row, column];
            for (var x = row + 1; x < Rows; x++)
            {
                if (TreeHeight[x, column] >= height)
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsVisibleWest(int row, int column)
        {
            var height = TreeHeight[row, column];
            for (var y = column - 1; y >= 0; y--)
            {
                if (TreeHeight[row, y] >= height)
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsVisibleEast(int row, int column)
        {
            var height = TreeHeight[row, column];
            for (var y = column + 1; y < Columns; y++)
            {
                if (TreeHeight[row, y] >= height)
                {
                    return false;
                }
            }

            return true;
        }
    }
}