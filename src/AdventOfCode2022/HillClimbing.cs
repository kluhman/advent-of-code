using System.Collections;
using System.Threading.Tasks.Dataflow;
using AdventOfCode.Core;

namespace AdventOfCode2022;

public class HillClimbing : IChallenge
{
    public int ChallengeId => 12;

    public object SolvePart1(string input)
    {
        var heightMap = HeightMap.Parse(input);
        return CalculateMinimumDistance(heightMap, heightMap.StartingPosition);
    }

    public object SolvePart2(string input)
    {
        var heightMap = HeightMap.Parse(input);
        var potentialStartingPoints = heightMap.Where(coordinates => heightMap[coordinates] == 0).ToList();

        var minDistance = int.MaxValue;
        var options = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded };
        var calculator = new TransformBlock<Coordinates, int>(coordinates => CalculateMinimumDistance(heightMap, coordinates), options);
        var reducer = new ActionBlock<int>(distance => minDistance = int.Min(minDistance, distance));
        calculator.LinkTo(reducer);

        potentialStartingPoints.ForEach(coordinates => calculator.Post(coordinates));
        calculator.Complete();
        calculator.Completion.Wait();

        return minDistance;
    }

    // calculate shortest path using Djiksta's algorithm
    // https://www.freecodecamp.org/news/dijkstras-shortest-path-algorithm-visual-introduction/
    private static int CalculateMinimumDistance(HeightMap heightMap, Coordinates startPosition)
    {
        var position = startPosition;
        var unvisitedNodes = new HashSet<Coordinates>(heightMap);
        var distance = new Dictionary<Coordinates, int> { { startPosition, 0 } };

        while (unvisitedNodes.Contains(heightMap.Destination))
        {
            unvisitedNodes.Remove(position);

            var adjacentNodes = heightMap.GetPotentialMoves(position);
            foreach (var adjacentNode in adjacentNodes)
            {
                var newDistance = distance[position] + 1;
                if (!distance.TryGetValue(adjacentNode, out var oldDistance) || newDistance < oldDistance)
                {
                    distance[adjacentNode] = newDistance;
                }
            }

            var nextPosition = unvisitedNodes.Where(distance.ContainsKey).MinBy(x => distance[x]);
            if (nextPosition is null)
            {
                break;
            }

            position = nextPosition;
        }

        return distance.TryGetValue(heightMap.Destination, out var minDistance) ? minDistance : int.MaxValue;
    }

    private record Coordinates(int Row, int Column);

    private class HeightMap : IEnumerable<Coordinates>
    {
        private readonly int[,] _map;

        public HeightMap(int[,] map, Coordinates startingPosition, Coordinates destination)
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

        public IEnumerable<Coordinates> GetPotentialMoves(Coordinates position)
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