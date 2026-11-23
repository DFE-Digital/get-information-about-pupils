using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace DfE.GIAP.Verification.Comparison;

/// <summary>
/// A kind of difference between the pipelines that has been explained and accepted, e.g. one of the deliberate
/// differences documented in <c>docs/file-ingestion.md</c>.
/// </summary>
/// <param name="Comparison">Omitted or <c>*</c> for any; the same for <paramref name="Dataset"/> and <paramref name="Field"/>.</param>
/// <param name="NewValue">A regular expression the whole of the new value (as JSON) must match; omitted for any.</param>
/// <param name="OldValue">A regular expression the whole of the old value (as JSON) must match; omitted for any.</param>
/// <param name="Reason">Why the difference is accepted.</param>
/// <param name="Docs">Where the difference is documented, e.g. <c>docs/file-ingestion.md#prepared-downloads</c>.</param>
public sealed record AcceptedDifferenceRule(
    string Kind,
    string? Comparison,
    string? Dataset,
    string? Field,
    string? NewValue,
    string? OldValue,
    string Reason,
    string Docs)
{
    public bool Matches(Difference difference) =>
        string.Equals(Kind, difference.Kind, StringComparison.Ordinal)
        && MatchesName(Comparison, difference.Comparison)
        && MatchesName(Dataset, difference.Dataset)
        && MatchesName(Field, difference.Field)
        && MatchesValue(NewValue, difference.NewValue)
        && MatchesValue(OldValue, difference.OldValue);

    private static bool MatchesName(string? pattern, string? value) =>
        pattern is null or "*" || string.Equals(pattern, value, StringComparison.OrdinalIgnoreCase);

    private static bool MatchesValue(string? pattern, string? value) =>
        pattern is null || Regex.IsMatch(value ?? "null", $"^(?:{pattern})$", RegexOptions.None, TimeSpan.FromSeconds(1));
}

/// <summary>
/// The rules in <c>accepted-differences.json</c>.
/// </summary>
public sealed class AcceptedDifferences
{
    public const string DefaultFileName = "accepted-differences.json";

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    private static readonly HashSet<string> _kinds = [.. typeof(DifferenceKinds)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Select(f => (string)f.GetRawConstantValue()!)];

    public AcceptedDifferences(IReadOnlyList<AcceptedDifferenceRule> rules)
    {
        ArgumentNullException.ThrowIfNull(rules);

        foreach (AcceptedDifferenceRule rule in rules)
        {
            if (!_kinds.Contains(rule.Kind))
            {
                throw new ArgumentException($"'{rule.Kind}' isn't a kind of difference.", nameof(rules));
            }

            if (string.IsNullOrWhiteSpace(rule.Reason) || string.IsNullOrWhiteSpace(rule.Docs))
            {
                throw new ArgumentException($"Every accepted difference needs a reason and docs; the '{rule.Kind}' rule doesn't.", nameof(rules));
            }
        }

        Rules = rules;
    }

    public IReadOnlyList<AcceptedDifferenceRule> Rules { get; }

    public static AcceptedDifferences Load(string path)
    {
        using FileStream stream = File.OpenRead(path);
        RulesFile file = JsonSerializer.Deserialize<RulesFile>(stream, _jsonOptions)
            ?? throw new InvalidOperationException($"'{path}' has no rules.");

        return new AcceptedDifferences(file.Rules);
    }

    /// <returns>The first rule that accepts the difference, if any.</returns>
    public AcceptedDifferenceRule? FindRule(Difference difference) => Rules.FirstOrDefault(r => r.Matches(difference));

    private sealed record RulesFile(IReadOnlyList<AcceptedDifferenceRule> Rules);
}
