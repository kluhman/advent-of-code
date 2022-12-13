namespace AdventOfCode.Core.PathFinding.Exceptions;

public class NoPathFoundException : Exception
{
    public NoPathFoundException(object start, object end) : base($"No path could be found from {start} to {end}")
    {
        Start = start;
        End = end;
    }

    public object Start { get; }
    public object End { get; }
}