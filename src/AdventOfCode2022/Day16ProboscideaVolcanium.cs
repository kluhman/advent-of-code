using System.Text.RegularExpressions;
using AdventOfCode.Core;
using AdventOfCode.Core.PathFinding;

namespace AdventOfCode2022;

public class Day16ProboscideaVolcanium : IChallenge
{
    private const int TimeToOpen = 1;

    public int ChallengeId => 16;

    public object SolvePart1(string input)
    {
        var graph = ParseValveGraph(input);
        var weightedGraph = ConvertToWeightedGraph(graph);
        var start = FindStartingValve(weightedGraph);

        var actors = new[] { new Actor(start, 30) };
        return CalculateMaxPressureRelease(weightedGraph, actors, weightedGraph.Nodes.Where(x => x.FlowRate > 0).ToArray());
    }

    public object SolvePart2(string input)
    {
        var graph = ParseValveGraph(input);
        var weightedGraph = ConvertToWeightedGraph(graph);
        var start = weightedGraph.Nodes.Single(x => x.Id == "AA");

        var actors = new[] { new Actor(start, 26), new Actor(start, 26) };
        return CalculateMaxPressureRelease(weightedGraph, actors, weightedGraph.Nodes.Where(x => x.FlowRate > 0).ToArray());
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

    private static Valve FindStartingValve(WeightedGraph<Valve> weightedGraph)
    {
        return weightedGraph.Nodes.Single(x => x.Id == "AA");
    }

    private static int CalculateMaxPressureRelease(WeightedGraph<Valve> graph, IReadOnlyCollection<Actor> actors, IReadOnlyCollection<Valve> usefulValves)
    {
        // find the actor with the most available time
        var actor = actors.OrderByDescending(x => x.MinutesRemaining).First();

        var max = 0;
        var valvesGroupedByDistance = usefulValves.GroupBy(valve => graph.GetEdges(actor.CurrentValve).Single(x => x.To == valve).Weight);

        foreach (var group in valvesGroupedByDistance)
        {
            var travelTime = group.Key;
            var timeRequired = travelTime + TimeToOpen;
            var valve = group.OrderByDescending(x => x.FlowRate).First();

            if (actor.MinutesRemaining < timeRequired)
            {
                continue;
            }

            var minutesRemaining = actor.MinutesRemaining - timeRequired;
            var pressureReleased = minutesRemaining * valve.FlowRate;

            var newActors = actors.Select(x => actor == x ? new Actor(valve, minutesRemaining) : x).ToArray();
            max = int.Max(max, pressureReleased + CalculateMaxPressureRelease(graph, newActors, usefulValves.Where(x => x != valve).ToArray()));
        }

        return max;
    }

    private record Valve(string Id, int FlowRate);

    private class Actor
    {
        public Actor(Valve currentValve, int minutesRemaining)
        {
            CurrentValve = currentValve;
            MinutesRemaining = minutesRemaining;
        }

        public Valve CurrentValve { get; }
        public int MinutesRemaining { get; }
    }
}
