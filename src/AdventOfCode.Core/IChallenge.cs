namespace AdventOfCode.Core;

public interface IChallenge
{
    int ChallengeId { get; }
    object SolvePart1(string input);
    object SolvePart2(string input);
}