using System.Collections.Immutable;
using System.Text.RegularExpressions;
using AdventOfCode.Core;
using AdventOfCode.Core.Extensions;
using AdventOfCode.Core.Models;

namespace AdventOfCode2023;

public partial class Day19Aplenty : IChallenge
{
    private const int MinPartValue = 1;
    private const int MaxPartValue = 4000;
    private const string InitialWorkflow = "in";
    private const string Accepted = "A";
    private const string Rejected = "R";

    public int ChallengeId => 19;

    public object SolvePart1(string input)
    {
        var lines = input.GetLines(StringSplitOptions.TrimEntries).ToList();
        var dividerIndex = lines.IndexOf(string.Empty);
        var parts = ParseParts(lines.Skip(dividerIndex));
        var workflows = lines.Take(dividerIndex).Select(Workflow.Parse).ToImmutableDictionary(x => x.Label, x => x);

        var sum = 0L;
        foreach (var part in parts)
        {
            var workflow = workflows[InitialWorkflow];

            while (true)
            {
                var result = workflow.Evaluate(part);
                if (result == Rejected)
                {
                    break;
                }

                if (result == Accepted)
                {
                    sum += part.Values.Sum();
                    break;
                }

                workflow = workflows[result];
            }
        }

        return sum;
    }

    public object SolvePart2(string input)
    {
        var lines = input.GetLines(StringSplitOptions.TrimEntries).ToList();
        var dividerIndex = lines.IndexOf(string.Empty);
        var workflows = lines.Take(dividerIndex).Select(Workflow.Parse).ToImmutableDictionary(x => x.Label, x => x);

        var partCombos = new Dictionary<string, Range<long>?>
        {
            { "x", new Range<long>(MinPartValue, MaxPartValue) },
            { "m", new Range<long>(MinPartValue, MaxPartValue) },
            { "a", new Range<long>(MinPartValue, MaxPartValue) },
            { "s", new Range<long>(MinPartValue, MaxPartValue) }
        }.ToImmutableDictionary();

        return GetMaxAcceptedParts(workflows, workflows[InitialWorkflow], partCombos);
    }

    private long GetMaxAcceptedParts(IReadOnlyDictionary<string, Workflow> workflows, Workflow workflow, ImmutableDictionary<string, Range<long>?> partCombos)
    {
        var maxAccepted = 0L;
        foreach (var rule in workflow.Rules)
        {
            // set part to the valid range of values for this rule and calculate the max combinations possible from this rule
            var validRange = partCombos[rule.Key]?.GetIntersection(rule.GetValidRange());
            maxAccepted += CalculateCombinations(workflows, partCombos.SetItem(rule.Key, validRange), rule.Result);

            // we've calculated all the combos that pass this rule, now set the invalid range so we can process fallbacks
            var invalidRange = partCombos[rule.Key]?.GetIntersection(rule.GetInvalidRange());
            partCombos = partCombos.SetItem(rule.Key, invalidRange);
        }

        // all rules have been evaluated, process the final fallback rule
        maxAccepted += CalculateCombinations(workflows, partCombos, workflow.FallbackResult);

        return maxAccepted;
    }

    private long CalculateCombinations(IReadOnlyDictionary<string, Workflow> workflows, ImmutableDictionary<string, Range<long>?> partCombos,
        string nextWorkflowKey) => nextWorkflowKey switch
    {
        // in terminal accepted state, accept all valid values in range
        Accepted => partCombos.Values.Aggregate(1L, (value, range) => range is null ? 0 : value * (range.End - range.Start + 1)),

        // in terminal rejected state, return 0
        Rejected => 0,

        // take the new valid ranges and send them to the next workflow
        _ => GetMaxAcceptedParts(workflows, workflows[nextWorkflowKey], partCombos)
    };

    private static IEnumerable<Dictionary<string, long>> ParseParts(IEnumerable<string> lines)
    {
        var valueRegex = PartRegex();
        foreach (var line in lines)
        {
            yield return valueRegex
                .Matches(line)
                .ToDictionary(x => x.Groups["key"].Value, x => long.Parse(x.Groups["value"].Value));
        }
    }

    [GeneratedRegex(@"(?<label>\w+)\{(?<rules>.+)\}")]
    private static partial Regex WorkflowRegex();

    [GeneratedRegex("(?<key>\\w)(?<op>\\<|\\>)(?<value>\\d+):(?<result>\\w+)")]
    private static partial Regex RuleRegex();

    [GeneratedRegex("(?<key>\\w)\\=(?<value>\\d+)")]
    private static partial Regex PartRegex();

    private class Workflow
    {
        public Workflow(string label, IReadOnlyList<Rule> rules, string fallbackResult)
        {
            Label = label;
            Rules = rules;
            FallbackResult = fallbackResult;
        }

        public string Label { get; }
        public IReadOnlyList<Rule> Rules { get; }
        public string FallbackResult { get; }

        public string Evaluate(IReadOnlyDictionary<string, long> part)
        {
            foreach (var rule in Rules)
            {
                if (rule.IsMatch(part))
                {
                    return rule.Result;
                }
            }

            return FallbackResult;
        }

        public static Workflow Parse(string line)
        {
            var match = WorkflowRegex().Match(line);
            var label = match.Groups["label"].Value;
            var splitRules = match.Groups["rules"].Value.Split(',');
            var rules = splitRules[..^1].Select(Rule.Parse).ToImmutableArray();
            var fallbackResult = splitRules.Last();

            return new Workflow(label, rules, fallbackResult);
        }
    }

    private record Rule(string Key, string Operation, long Value, string Result)
    {
        public bool IsMatch(IReadOnlyDictionary<string, long> part)
        {
            var partValue = part.GetValueOrDefault(Key);
            return Operation switch
            {
                ">" => partValue > Value,
                "<" => partValue < Value,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public Range<long> GetValidRange() => Operation switch
        {
            ">" => new Range<long>(Value + 1, MaxPartValue),
            "<" => new Range<long>(MinPartValue, Value - 1),
            _ => throw new ArgumentOutOfRangeException()
        };

        public Range<long> GetInvalidRange() => Operation switch
        {
            ">" => new Range<long>(MinPartValue, Value),
            "<" => new Range<long>(Value, MaxPartValue),
            _ => throw new ArgumentOutOfRangeException()
        };

        public static Rule Parse(string line)
        {
            var ruleRegex = RuleRegex();
            var match = ruleRegex.Match(line);

            return new Rule(match.Groups["key"].Value, match.Groups["op"].Value, long.Parse(match.Groups["value"].Value), match.Groups["result"].Value);
        }
    }
}
