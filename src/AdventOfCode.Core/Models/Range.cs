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

    public Range<T>? GetIntersection(Range<T> other)
    {
        if (Contains(other))
        {
            return other;
        }

        if (Contains(other.Start))
        {
            return new Range<T>(other.Start, End);
        }

        if (Contains(other.End))
        {
            return new Range<T>(Start, other.End);
        }

        if (other.Contains(this))
        {
            return this;
        }

        if (other.Contains(Start))
        {
            return new Range<T>(Start, other.End);
        }

        if (other.Contains(End))
        {
            return new Range<T>(other.Start, End);
        }

        return null;
    }
}
