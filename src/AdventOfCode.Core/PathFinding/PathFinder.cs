using AdventOfCode.Core.PathFinding.Exceptions;

namespace AdventOfCode.Core.PathFinding;

public class PathFinder
{
    // calculate shortest path using Breadth First Search, since this graph is unweighted
    // https://en.wikipedia.org/wiki/Breadth-first_search
    public static int FindShortestPath<T>(Graph<T> graph, T start, T destination) where T : notnull
    {
        var visited = new HashSet<T> { start };
        var toVisit = new Queue<(T node, int distance)>(new[] { (start, 0) });

        while (toVisit.Any())
        {
            var (node, distance) = toVisit.Dequeue();
            if (node.Equals(destination))
            {
                return distance;
            }

            foreach (var adjacentNode in graph.Edges[node])
            {
                if (visited.Contains(adjacentNode.To))
                {
                    continue;
                }

                visited.Add(adjacentNode.To);
                toVisit.Enqueue((adjacentNode.To, distance + 1));
            }
        }

        throw new NoPathFoundException(start, destination);
    }

    // calculate shortest path using Djiksta's algorithm
    // https://www.freecodecamp.org/news/dijkstras-shortest-path-algorithm-visual-introduction/
    public static int FindShortestPath<T>(WeightedGraph<T> graph, T start, T destination) where T : notnull
    {
        var position = start;
        var unvisitedNodes = new HashSet<T>(graph.Nodes);
        var distance = new Dictionary<T, int> { { start, 0 } };

        while (unvisitedNodes.Contains(destination))
        {
            unvisitedNodes.Remove(position);

            foreach (var edge in graph.Edges[position])
            {
                var newDistance = distance[position] + edge.Weight;
                if (!distance.TryGetValue(edge.To, out var oldDistance) || newDistance < oldDistance)
                {
                    distance[edge.To] = newDistance;
                }
            }

            var nextPosition = unvisitedNodes.Where(distance.ContainsKey).MinBy(x => distance[x]);
            if (nextPosition is null)
            {
                break;
            }

            position = nextPosition;
        }

        if (!distance.TryGetValue(destination, out var distanceToDestination))
        {
            throw new NoPathFoundException(start, destination);
        }

        return distanceToDestination;
    }
}