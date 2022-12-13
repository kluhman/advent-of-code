using System.Collections;
using System.Threading.Tasks.Dataflow;
using AdventOfCode.Core;
using AdventOfCode.Core.PathFinding;
using AdventOfCode.Core.PathFinding.Exceptions;

namespace AdventOfCode2022;

public class HillClimbing : IChallenge
{
    public int ChallengeId => 12;

    public object SolvePart1(string input)
    {
        var heightMap = HeightMap.Parse(input);
        var graph = heightMap.ToGraph();
        var weightedGraph = heightMap.ToWeightedGraph();

        return GitHub.Scientist.Science<int>("Calculate Shortest Path from Start", experiment =>
        {
            experiment.Use(() => CalculateMinimumDistance(graph, heightMap.StartingPosition, heightMap.Destination));
            experiment.Try("Using Weighted Graph", () => CalculateMinimumDistance(weightedGraph, heightMap.StartingPosition, heightMap.Destination));
        });
    }

    public object SolvePart2(string input)
    {
        var heightMap = HeightMap.Parse(input);
        var graph = heightMap.ToGraph();
        var weightedGraph = heightMap.ToWeightedGraph();

        var destination = heightMap.Destination;
        var potentialStartingPositions = heightMap.Where(coordinates => heightMap[coordinates] == 0).ToArray();
        return GitHub.Scientist.Science<int>("Calculate Shortest Hiking Trail", experiment =>
        {
            experiment.Use(() => FindShortestTrail(potentialStartingPositions, start => CalculateMinimumDistance(graph, start, destination)));
            experiment.Try("Using Weighted Graph", () => FindShortestTrail(potentialStartingPositions,
                start => CalculateMinimumDistance(weightedGraph, start, destination)));
        });
    }

    private static int CalculateMinimumDistance(Graph<Coordinates> graph, Coordinates start, Coordinates destination)
    {
        try
        {
            return PathFinder.FindShortestPath(graph, start, destination);
        }
        catch (NoPathFoundException)
        {
            return int.MaxValue;
        }
    }

    private static int CalculateMinimumDistance(WeightedGraph<Coordinates> graph, Coordinates start, Coordinates destination)
    {
        try
        {
            return PathFinder.FindShortestPath(graph, start, destination);
        }
        catch (NoPathFoundException)
        {
            return int.MaxValue;
        }
    }

    private static int FindShortestTrail(IEnumerable<Coordinates> potential, Func<Coordinates, int> calculateShortestDistance)
    {
        var minDistance = int.MaxValue;
        var options = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded };
        var calculator = new TransformBlock<Coordinates, int>(calculateShortestDistance, options);
        var reducer = new ActionBlock<int>(distance => minDistance = int.Min(minDistance, distance));
        calculator.LinkTo(reducer);

        foreach (var potentialStartingPoint in potential)
        {
            calculator.Post(potentialStartingPoint);
        }

        calculator.Complete();
        calculator.Completion.Wait();

        return minDistance;
    }

    private record Coordinates(int Row, int Column);

    private class HeightMap : IEnumerable<Coordinates>
    {
        private readonly int[,] _map;

        private HeightMap(int[,] map, Coordinates startingPosition, Coordinates destination)
        {
            _map = map;
            StartingPosition = startingPosition;
            Destination = destination;
        }

        public int this[Coordinates coordinates] => _map[coordinates.Row, coordinates.Column];
        public int Rows => _map.GetLength(0);
        public int Columns => _map.GetLength(1);
        public Coordinates StartingPosition { get; }
        public Coordinates Destination { get; }

        public IEnumerator<Coordinates> GetEnumerator() => GetAllCoordinates().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public Graph<Coordinates> ToGraph()
        {
            var edges = from position in GetAllCoordinates()
                from adjacentPosition in GetPotentialMoves(position)
                select new Edge<Coordinates>(position, adjacentPosition);

            return new Graph<Coordinates>(GetAllCoordinates(), edges);
        }

        public WeightedGraph<Coordinates> ToWeightedGraph()
        {
            var edges = from position in GetAllCoordinates()
                from adjacentPosition in GetPotentialMoves(position)
                select new WeightedEdge<Coordinates>(position, adjacentPosition, 1);

            return new WeightedGraph<Coordinates>(GetAllCoordinates(), edges);
        }

        private IEnumerable<Coordinates> GetAllCoordinates()
        {
            for (var row = 0; row < Rows; row++)
            {
                for (var column = 0; column < Columns; column++)
                {
                    yield return new Coordinates(row, column);
                }
            }
        }

        private IEnumerable<Coordinates> GetPotentialMoves(Coordinates position)
        {
            var maxHeight = _map[position.Row, position.Column] + 1;

            var left = CheckLeft(position);
            if (left <= maxHeight)
            {
                yield return position with { Column = position.Column - 1 };
            }

            var right = CheckRight(position);
            if (right <= maxHeight)
            {
                yield return position with { Column = position.Column + 1 };
            }

            var up = CheckUp(position);
            if (up <= maxHeight)
            {
                yield return position with { Row = position.Row - 1 };
            }

            var down = CheckDown(position);
            if (down <= maxHeight)
            {
                yield return position with { Row = position.Row + 1 };
            }
        }

        private int? CheckLeft(Coordinates position) => position.Column == 0 ? null : _map[position.Row, position.Column - 1];
        private int? CheckRight(Coordinates position) => position.Column == Columns - 1 ? null : _map[position.Row, position.Column + 1];
        private int? CheckUp(Coordinates position) => position.Row == 0 ? null : _map[position.Row - 1, position.Column];
        private int? CheckDown(Coordinates position) => position.Row == Rows - 1 ? null : _map[position.Row + 1, position.Column];

        public static HeightMap Parse(string input)
        {
            var lines = input.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            var start = default(Coordinates);
            var end = default(Coordinates);
            var rows = lines.Length;
            var columns = lines[0].Length;
            var map = new int[rows, columns];
            for (var row = 0; row < rows; row++)
            {
                for (var column = 0; column < columns; column++)
                {
                    var character = lines[row][column];
                    switch (character)
                    {
                        case 'S':
                            map[row, column] = 0;
                            start = new Coordinates(row, column);
                            break;
                        case 'E':
                            map[row, column] = 25;
                            end = new Coordinates(row, column);
                            break;
                        default:
                            map[row, column] = lines[row][column] - 'a';
                            break;
                    }
                }
            }

            return new HeightMap(map, start!, end!);
        }
    }
}