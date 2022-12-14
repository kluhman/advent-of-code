namespace AdventOfCode.Core.Models;

public record Range<T> where T : IComparable<T>
{
    public Range(T start, T end)
    {
        if (start.CompareTo(end) > 0)
        {
            throw new ArgumentException($"End '{end}' must be equal to or greater than Start '{start}");
        }

        Start = start;
        End = end;
    }

    public T Start { get; }
    public T End { get; }

    public bool Contains(T value)
    {
        var afterStart = value.CompareTo(Start) >= 0;
        var beforeEnd = value.CompareTo(End) <= 0;
        return afterStart && beforeEnd;
    }

    public bool Contains(Range<T> range) => Contains(range.Start) && Contains(range.End);
    public bool Intersects(Range<T> range) => Contains(range.Start) || Contains(range.End) || range.Contains(Start) || range.Contains(End);
}