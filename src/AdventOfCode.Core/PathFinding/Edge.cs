namespace AdventOfCode.Core.PathFinding;

public record Edge<TNode>(TNode From, TNode To);

public record WeightedEdge<TNode>(TNode From, TNode To, int Weight) : Edge<TNode>(From, To);
