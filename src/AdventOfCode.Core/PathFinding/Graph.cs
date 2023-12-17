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
    private readonly Func<TNode, IEnumerable<WeightedEdge<TNode>>> _getNodes;

    public WeightedGraph(IEnumerable<TNode> nodes, IEnumerable<WeightedEdge<TNode>> edges)
    {
        var lookup = edges.ToLookup(x => x.From);
        Nodes = nodes.ToHashSet();
        _getNodes = node => lookup[node];
    }

    public WeightedGraph(IEnumerable<TNode> nodes, Func<TNode, IEnumerable<WeightedEdge<TNode>>> getEdges)
    {
        Nodes = nodes.ToHashSet();
        _getNodes = getEdges;
    }

    public IReadOnlySet<TNode> Nodes { get; }
    public IEnumerable<WeightedEdge<TNode>> GetEdges(TNode node) => _getNodes(node);
}
