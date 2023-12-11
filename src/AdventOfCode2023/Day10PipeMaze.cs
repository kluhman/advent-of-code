using System.Collections.Immutable;
using System.Text.RegularExpressions;
using AdventOfCode.Core;
using AdventOfCode.Core.Extensions;

namespace AdventOfCode2023;

public partial class Day10PipeMaze : IChallenge
{
    public int ChallengeId => 10;

    public object SolvePart1(string input)
    {
        var maze = Maze.Parse(input);
        var maxSteps = TraverseLoop(maze, out _);

        return maxSteps;
    }

    public object SolvePart2(string input)
    {
        var maze = Maze.Parse(input);
        TraverseLoop(maze, out var pipes);

        var boundaries = pipes.Select(x => x.ToSpace()).ToHashSet();
        PrintMaze(maze, boundaries);

        var unknown = Enumerable
            .Range(0, maze.Width)
            .SelectMany(x => Enumerable.Range(0, maze.Height).Select(y => new Space(x, y)))
            .Except(boundaries)
            .ToList();

        var boundaryLookup = pipes.ToLookup(x => x.Y);
        return unknown.Count(space => IsEnclosed(boundaryLookup, space));
    }

    private static int TraverseLoop(Maze maze, out HashSet<Pipe> pipes)
    {
        pipes = new HashSet<Pipe>(new[] { maze.Start });
        var queue = new Queue<(Pipe pipe, int steps)>(GetInitialConnections(maze).Select(pipe => (pipe, 1)));

        var maxSteps = 0;
        while (queue.TryDequeue(out var current))
        {
            var (currentPipe, currentSteps) = current;

            pipes.Add(currentPipe);
            maxSteps = int.Max(currentSteps, maxSteps);

            var connections = currentPipe.GetConnectingSpaces().Select(maze.Get).OfType<Pipe>();
            foreach (var connection in connections)
            {
                if (pipes.Contains(connection))
                {
                    continue;
                }

                queue.Enqueue((connection, currentSteps + 1));
            }
        }

        return maxSteps;
    }

    private static IEnumerable<Pipe> GetInitialConnections(Maze maze) => maze
        .Start
        .GetConnectingSpaces()
        .Select(maze.Get)
        .OfType<Pipe>()
        .Where(connection => connection.GetConnectingSpaces().Contains(new Space(maze.Start.X, maze.Start.Y)));

    private static bool IsEnclosed(ILookup<int, Pipe> boundaries, Space space)
    {
        var pipes = new string(boundaries[space.Y]
            .Where(boundary => boundary.X > space.X)
            .OrderBy(boundary => boundary.X)
            .Select(pipe => pipe.Shape)
            .ToArray());

        pipes = pipes.Replace("S", "─");
        pipes = SafeCorners().Replace(pipes, string.Empty);
        pipes = IntersectingCorners().Replace(pipes, "│");

        var intersections = pipes.Count(x => x == '│');

        // if there is an odd number of intersections, then it is enclosed
        return intersections % 2 == 1;
    }

    private static void PrintMaze(Maze maze, IReadOnlySet<Space> boundaries)
    {
        for (var y = maze.Height - 1; y >= 0; y--)
        {
            for (var x = 0; x < maze.Width; x++)
            {
                var space = new Space(x, y);
                Console.Write(boundaries.Contains(space)
                    ? ((Pipe)maze.Get(space)!).Shape
                    : '.');
            }

            Console.WriteLine();
        }
    }

    [GeneratedRegex("┌─*┐|└─*┘", RegexOptions.Compiled)]
    private static partial Regex SafeCorners();

    [GeneratedRegex("┌─*┘|└─*┐", RegexOptions.Compiled)]
    private static partial Regex IntersectingCorners();

    private class Maze
    {
        private Maze(Pipe start, int width, int height, Space[,] spaces)
        {
            Start = start;
            Width = width;
            Height = height;
            Spaces = spaces;
        }

        public Pipe Start { get; }
        public int Width { get; }
        public int Height { get; }
        public Space[,] Spaces { get; }

        public Space? Get(Space space)
        {
            if (space.X < 0 || space.X >= Width)
            {
                return null;
            }

            if (space.Y < 0 || space.Y >= Height)
            {
                return null;
            }

            return Spaces[space.Y, space.X];
        }

        public static Maze Parse(string input)
        {
            var lines = input
                .Replace('|', '│')
                .Replace('-', '─')
                .Replace('L', '└')
                .Replace('J', '┘')
                .Replace('7', '┐')
                .Replace('F', '┌')
                .GetLines()
                .Reverse()
                .ToImmutableArray();

            var start = default(Pipe);
            var height = lines.Length;
            var width = lines[0].Length;
            var maze = new Space[height, width];
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var value = lines[y][x];
                    maze[y, x] = value == '.'
                        ? new Space(x, y)
                        : new Pipe(x, y, value);

                    if (value == 'S')
                    {
                        start = maze[y, x] as Pipe;
                    }
                }
            }

            return new Maze(start!, width, height, maze);
        }
    }

    private record Space(int X, int Y)
    {
        public Space North => new(X, Y + 1);
        public Space East => new(X + 1, Y);
        public Space South => new(X, Y - 1);
        public Space West => new(X - 1, Y);

        public IEnumerable<Space> Adjacent => new[] { North, East, South, West };
    }

    private record Pipe(int X, int Y, char Shape) : Space(X, Y)
    {
        public Space ToSpace() => new(X, Y);

        public IEnumerable<Space> GetConnectingSpaces() => Shape switch
        {
            'S' => new[] { North, East, South, West },
            '│' => new[] { North, South },
            '─' => new[] { East, West },
            '└' => new[] { North, East },
            '┘' => new[] { North, West },
            '┐' => new[] { West, South },
            '┌' => new[] { South, East },
            _ => Array.Empty<Space>()
        };
    }
}
