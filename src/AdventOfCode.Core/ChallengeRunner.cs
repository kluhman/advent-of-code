using System.Diagnostics;
using System.Reflection;
using Scientist.Publishers.Console;
using Spectre.Console;

namespace AdventOfCode.Core;

public interface IChallengeRunner
{
    void Run(int challengeId, string input, Assembly assembly);
}

public class ChallengeRunner : IChallengeRunner
{
    public void Run(int challengeId, string input, Assembly assembly)
    {
        GitHub.Scientist.ResultPublisher = new ConsoleResultPublisher();

        var challenges = assembly
            .GetTypes()
            .Where(x => x.IsAssignableTo(typeof(IChallenge)) && !x.IsAbstract)
            .Select(Activator.CreateInstance)
            .Cast<IChallenge>()
            .ToList();

        var challenge = challenges.SingleOrDefault(x => x.ChallengeId == challengeId);
        if (challenge is null)
        {
            AnsiConsole.MarkupLine(
                $"[red]No challenge with id '{challengeId}' could be found. Please verify the challenge has been implemented, and declares the correct id[/]");
            return;
        }

        try
        {
            var stopwatch = Stopwatch.StartNew();
            var solution1 = challenge.SolvePart1(input);
            stopwatch.Stop();

            AnsiConsole.MarkupLine($"[blue]Solved Part 1 in: {stopwatch.Elapsed}[/]");
            AnsiConsole.MarkupLine($"[green]The solution to Part 1 is: {solution1}[/]");
            AnsiConsole.WriteLine();

            stopwatch.Restart();
            var solution2 = challenge.SolvePart2(input);
            stopwatch.Stop();

            AnsiConsole.MarkupLine($"[blue]Solved Part 2 in: {stopwatch.Elapsed}[/]");
            AnsiConsole.MarkupLine($"[green]The solution to Part 2 is: {solution2}[/]");
            AnsiConsole.WriteLine();
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
        }
    }
}