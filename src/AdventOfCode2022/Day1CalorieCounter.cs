using System.Text;
using AdventOfCode.Core;

namespace AdventOfCode2022;

internal class Day1CalorieCounter : IChallenge
{
    public int ChallengeId => 1;

    public object SolvePart1(string input)
    {
        var elf = 1;
        var maxElf = 1;
        var maxCalories = 0;
        var currentCalories = 0;
        var lines = input.Split("\n", StringSplitOptions.TrimEntries);

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                CheckForHigherCalorieCount(elf, currentCalories, ref maxElf, ref maxCalories);

                elf++;
                currentCalories = 0;
                continue;
            }

            if (!int.TryParse(line, out var calories))
            {
                throw new Exception(
                    $"Line value '{line}' is not a valid input for the snack list. Input should be a integer of calories");
            }

            currentCalories += calories;
        }

        CheckForHigherCalorieCount(elf, currentCalories, ref maxElf, ref maxCalories);

        return maxCalories;
    }

    public object SolvePart2(string input)
    {
        var results = CountCalories(input);
        return SummarizeResults(results);
    }

    private static void CheckForHigherCalorieCount(int elf, int currentCalories, ref int maxElf, ref int maxCalories)
    {
        if (currentCalories > maxCalories)
        {
            maxCalories = currentCalories;
            maxElf = elf;
        }
    }

    private static List<SnackCount> CountCalories(string input)
    {
        var elf = 1;
        var currentCalories = 0;
        var results = new List<SnackCount>();
        var lines = input.Split("\n", StringSplitOptions.TrimEntries);

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                results.Add(new SnackCount(elf, currentCalories));

                elf++;
                currentCalories = 0;
                continue;
            }

            if (!int.TryParse(line, out var calories))
            {
                throw new Exception(
                    $"Line value '{line}' is not a valid input for the snack list. Input should be a integer of calories");
            }

            currentCalories += calories;
        }

        results.Sort(SnackCount.Default);

        return results;
    }

    private static object SummarizeResults(List<SnackCount> results)
    {
        var totalCalories = 0;
        var topElves = results.Take(3);
        var output = new StringBuilder("\n");

        foreach (var elf in topElves)
        {
            totalCalories += elf.Calories;
            output.AppendLine(elf.ToString());
        }

        output.AppendLine($"Total: {totalCalories}");
        return output.ToString();
    }

    private record SnackCount(int Elf, int Calories)
    {
        public static readonly IComparer<SnackCount> Default =
            Comparer<SnackCount>.Create((left, right) => -left.Calories.CompareTo(right.Calories));

        public override string ToString() => $"Elf #{Elf}, Calories = {Calories}";
    }
}