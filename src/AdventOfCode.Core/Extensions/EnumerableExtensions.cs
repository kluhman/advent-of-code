using AdventOfCode.Core.Models;

namespace AdventOfCode.Core.Extensions;

public static class EnumerableExtensions
{
    public static Range<TOut>? ToRange<TIn, TOut>(this IEnumerable<TIn> items, Func<TIn, TOut> selector) where TOut : IComparable<TOut> =>
        items.Aggregate(default(Range<TOut>), (current, item) =>
        {
            var value = selector(item);
            if (current is null)
            {
                return new Range<TOut>(value, value);
            }

            if (value.CompareTo(current.Start) < 0)
            {
                current = new Range<TOut>(value, current.End);
            }

            if (value.CompareTo(current.End) > 0)
            {
                current = new Range<TOut>(current.Start, value);
            }

            return current;
        });
}