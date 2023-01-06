using System.Reflection;
using AdventOfCode.Core;
using Spectre.Console;

var (year, day) = args switch
{
    ["--auto"] => GetCurrentChallenge(),
    [var yearStr, var dayStr] => (int.Parse(yearStr), int.Parse(dayStr)),
    _ => PromptForChallenge()
};

if (!File.Exists("input.txt"))
{
    AnsiConsole.MarkupLine(
        @"[red]No input file could be found. Please add challenge input in a file called ""input.txt"" in the executing directory.[/]");
    return;
}

try
{
    var challengeAssembly = Assembly.Load($"AdventOfCode{year}");
    var runner = new ChallengeRunner();
    var input = await File.ReadAllTextAsync("input.txt");
    runner.Run(day, input, challengeAssembly);
}
catch (Exception ex) when (ex is FileLoadException or FileNotFoundException)
{
    AnsiConsole.MarkupLine($@"[red]Challenge assembly for year '{year}' could not be found[/]");
}

static (int year, int day) GetCurrentChallenge() => (DateTime.Now.Year, DateTime.Now.Day);

static (int year, int day) PromptForChallenge()
{
    const int minYear = 2015;
    var maxYear = DateTime.Now.Year;
    var year = AnsiConsole.Prompt(new TextPrompt<int>("Which year of Advent of Code are you working in?")
        .Validate(input =>
        {
            if (input < minYear || input > maxYear)
            {
                return ValidationResult.Error($"Year must be between {minYear} and {maxYear}");
            }

            return ValidationResult.Success();
        }));

    var day = AnsiConsole.Prompt(new TextPrompt<int>("Which challenge are you solving?")
        .Validate(input => input switch
        {
            < 1 or > 25 => ValidationResult.Error("Challenge must be between 1 and 25"),
            _ => ValidationResult.Success()
        }));

    return (year, day);
}