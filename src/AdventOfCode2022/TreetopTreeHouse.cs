using System.Drawing;
using AdventOfCode.Core;

namespace AdventOfCode2022
{
    internal class TreetopTreeHouse : IChallenge
    {
        public int ChallengeId => 8;

        public object SolvePart1(string input)
        {
            var (grid, rows, columns) = ParseTreeMap(input);

            var visibility = new bool[rows, columns];
            CheckVisibilityFromLeft(grid, rows, columns, visibility);
            CheckVisibilityFromRight(grid, rows, columns, visibility);
            CheckVisibilityFromTop(grid, rows, columns, visibility);
            CheckVisibilityFromBottom(grid, rows, columns, visibility);

            return CountVisibleTrees(rows, columns, visibility);
        }

        public object SolvePart2(string input)
        {
            var (grid, rows, columns) = ParseTreeMap(input);

            var highestScore = 0;
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    var startingPoint = new Point(row, column);
                    var scenicScore = CalculateScenicScore(grid, rows, columns, startingPoint);
                    if (scenicScore > highestScore)
                    {
                        highestScore = scenicScore;
                    }
                }
            }

            return highestScore;
        }

        private static (int[,] grid, int rows, int columns) ParseTreeMap(string input)
        {
            var lines = input.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            var rows = lines.Length;
            var columns = lines[0].Length;
            var grid = new int[rows, columns];
            for (int row = 0; row < lines.Length; row++)
            {
                var line = lines[row];
                for (int column = 0; column < line.Length; column++)
                {
                    grid[row, column] = int.Parse(line[column].ToString());
                }
            }

            return (grid, rows, columns);
        }

        private static void CheckVisibilityFromLeft(int[,] grid, int rows, int columns, bool[,] visibility)
        {
            for (int row = 0; row < rows; row++)
            {
                var maxHeight = -1;
                for (int column = 0; column < columns; column++)
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
            for (int row = 0; row < rows; row++)
            {
                var maxHeight = -1;
                for (int column = columns - 1; column >= 0; column--)
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
            for (int column = 0; column < columns; column++)
            {
                var maxHeight = -1;
                for (int row = 0; row < rows; row++)
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
            for (int column = 0; column < columns; column++)
            {
                var maxHeight = -1;
                for (int row = rows - 1; row >= 0; row--)
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
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
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
    }
}
