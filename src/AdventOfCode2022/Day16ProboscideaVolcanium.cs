using System.Text.RegularExpressions;
using AdventOfCode.Core;
using AdventOfCode.Core.PathFinding;

namespace AdventOfCode2022;

public class Day16ProboscideaVolcanium : IChallenge
{
    public int ChallengeId => 16;

    public object SolvePart1(string input)
    {
        var graph = ParseValveGraph(input);
        var weightedGraph = ConvertToWeightedGraph(graph);
        return GitHub.Scientist.Science<int>("Calculate Max Pressure Release Solo", experiment =>
        {
            experiment.Use(() =>
            {
                var start = weightedGraph.Nodes.Single(x => x.Id == "AA");
                return VisitValve(weightedGraph, start, new[] { start }, 30);
            });
        });
    }

    public object SolvePart2(string input)
    {
        var graph = ParseValveGraph(input);
        var weightedGraph = ConvertToWeightedGraph(graph);
        return GitHub.Scientist.Science<int>("Calculate Max Pressure Release Solo", experiment =>
        {
            experiment.Use(() =>
            {
                var start = weightedGraph.Nodes.Single(x => x.Id == "AA");
                return VisitValve(weightedGraph, start, new[] { start }, 26);
            });
        });
    }

    private static int VisitValve(WeightedGraph<Valve> graph, Valve valve, IEnumerable<Valve> visited, int minutesRemaining)
    {
        if (minutesRemaining <= 0)
        {
            return 0;
        }

        var pressureReleased = 0;
        if (valve.FlowRate > 0)
        {
            minutesRemaining--;
            pressureReleased = minutesRemaining * valve.FlowRate;
        }

        var newVisits = visited.Concat(new[] { valve }).ToHashSet();
        var nextValves = graph
            .Edges[valve]
            .Where(edge => edge.To.FlowRate > 0)
            .Where(edge => !newVisits.Contains(edge.To));

        var temp = 0;
        foreach (var (_, nextValve, travelTime) in nextValves)
        {
            temp = int.Max(temp, VisitValve(graph, nextValve, newVisits, minutesRemaining - travelTime));
        }

        return pressureReleased + temp;
    }

    private static Graph<Valve> ParseValveGraph(string input)
    {
        var valves = new List<Valve>();
        var connections = new Dictionary<Valve, IEnumerable<string>>();
        var matches = Regex.Matches(input, @"Valve (?<valveName>.+) has flow rate=(?<flowRate>\d+); tunnels? leads? to valves? ((?<connections>.+))");
        foreach (Match match in matches)
        {
            var valve = new Valve(match.Groups["valveName"].Value, int.Parse(match.Groups["flowRate"].Value));
            var connectionsToValve = match.Groups["connections"].Value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            valves.Add(valve);
            connections.Add(valve, connectionsToValve);
        }

        var edges = connections.SelectMany(pair => pair.Value.Select(connection =>
        {
            var connectingValve = valves.Single(x => x.Id == connection);
            return new Edge<Valve>(pair.Key, connectingValve);
        }));

        return new Graph<Valve>(valves, edges);
    }

    private static WeightedGraph<Valve> ConvertToWeightedGraph(Graph<Valve> graph)
    {
        var edges = graph
            .Nodes
            .SelectMany(start => graph
                .Nodes
                .Except(new[] { start })
                .Select(finish => new WeightedEdge<Valve>(start, finish, PathFinder.FindShortestPath(graph, start, finish))));

        return new WeightedGraph<Valve>(graph.Nodes, edges);
    }

    private record Valve(string Id, int FlowRate);
}