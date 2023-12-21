namespace AdventOfCode.Core.Models;

public record Point(long X, long Y)
{
    public Point Up => this with { Y = Y - 1 };
    public Point Down => this with { Y = Y + 1 };
    public Point Left => this with { X = X - 1 };
    public Point Right => this with { X = X + 1 };
    public IEnumerable<Point> Adjacent => new[] { Up, Down, Left, Right };
}
