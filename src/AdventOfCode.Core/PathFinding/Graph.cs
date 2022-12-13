namespace AdventOfCode.Core.PathFinding;

public class Graph<TNode> where TNode : notnull
{
    public Graph(IEnumerable<TNode> nodes, IEnumerable<Edge<TNode>> edges)
    {
        Nodes = nodes.ToHashSet();
        Edges = edges.ToLookup(x => x.From);
    }

    public IReadOnlySet<TNode> Nodes { get; }
    public ILookup<TNode, Edge<TNode>> Edges { get; }
}

public class WeightedGraph<TNode> where TNode : notnull
{
    public WeightedGraph(IEnumerable<TNode> nodes, IEnumerable<WeightedEdge<TNode>> edges)
    {
        Nodes = nodes.ToHashSet();
        Edges = edges.ToLookup(x => x.From);
    }

    public IReadOnlySet<TNode> Nodes { get; }
    public ILookup<TNode, WeightedEdge<TNode>> Edges { get; }
}
