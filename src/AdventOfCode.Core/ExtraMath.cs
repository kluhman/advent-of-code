namespace AdventOfCode.Core;

public static class ExtraMath
{
    /// <summary>
    ///     Solves a quadratic equation in the form of ax^2 + bx + c;
    /// </summary>
    /// <param name="a">Quadratic co-efficient</param>
    /// <param name="b">Linear co-efficient</param>
    /// <param name="c">Constant</param>
    /// <returns>Possible solutions</returns>
    public static (double x1, double x2) QuadraticFormula(double a, double b, double c)
    {
        // https://en.wikipedia.org/wiki/Quadratic_formula
        var x1 = (-b + Math.Sqrt(b * b - 4 * a * c)) / (2 * a);
        var x2 = (-b - Math.Sqrt(b * b - 4 * a * c)) / (2 * a);

        return (x1, x2);
    }

    /// <summary>
    ///     Finds the largest number that both values can be divided by
    /// </summary>
    public static long GreatestCommonDenominator(long left, long right) => right == 0
        ? left
        : GreatestCommonDenominator(right, left % right);

    /// <summary>
    ///     Finds the smallest number both values are factors of
    /// </summary>
    public static long LeastCommonMultiple(IEnumerable<long> numbers) => numbers
        .Aggregate((left, val) => left * val / GreatestCommonDenominator(left, val));
}
