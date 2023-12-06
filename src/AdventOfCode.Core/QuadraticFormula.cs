namespace AdventOfCode.Core;

public static class QuadraticFormula
{
    public static (double x1, double x2) Solve(double a, double b, double c)
    {
        // https://en.wikipedia.org/wiki/Quadratic_formula
        var x1 = (-b + Math.Sqrt(b * b - 4 * a * c)) / (2 * a);
        var x2 = (-b - Math.Sqrt(b * b - 4 * a * c)) / (2 * a);

        return (x1, x2);
    }
}
