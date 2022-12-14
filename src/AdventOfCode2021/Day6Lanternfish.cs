using AdventOfCode.Core;

namespace AdventOfCode2021;

public class Day6Lanternfish : IChallenge
{
    private const int DaysInCycle = 7;
    private const int NewFishCycleDelay = 2;
    private const int MaxDaysRemaining = DaysInCycle + NewFishCycleDelay;

    public int ChallengeId => 6;
    public object SolvePart1(string input) => SimulateSchoolGrowth(input, 80);
    public object SolvePart2(string input) => SimulateSchoolGrowth(input, 256);

    private static long SimulateSchoolGrowth(string input, int daysInSimulation)
    {
        var populationByAge = ParseFish(input);

        for (var day = 0; day < daysInSimulation; day++)
        {
            var numberOfFishInGeneration = populationByAge[0];

            // reduce the age of every generation by 1
            for (var age = 1; age < populationByAge.Length; age++)
            {
                populationByAge[age - 1] = populationByAge[age];
            }

            // add this generation to the reset of the cycle
            populationByAge[DaysInCycle - 1] += numberOfFishInGeneration;

            // add the children of this generation to the reset of the cycle + the delay period
            populationByAge[MaxDaysRemaining - 1] = numberOfFishInGeneration;
        }

        // sum the entire population
        return populationByAge.Sum();
    }

    private static long[] ParseFish(string input)
    {
        var ageArray = new long[MaxDaysRemaining];
        var ages = input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(int.Parse);
        foreach (var age in ages)
        {
            ageArray[age]++;
        }

        return ageArray;
    }
}