namespace AdventOfCode.Core.Extensions;

public static class StringExtensions
{
    private const StringSplitOptions DefaultSplitOptions = StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries;

    public static string[] GetLines(this string input, StringSplitOptions options = DefaultSplitOptions) =>
        input.Split('\n', options);
}
