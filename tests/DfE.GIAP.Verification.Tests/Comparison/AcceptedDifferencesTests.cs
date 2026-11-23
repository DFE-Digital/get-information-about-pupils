using DfE.GIAP.Verification.Comparison;

namespace DfE.GIAP.Verification.Tests.Comparison;

public class AcceptedDifferencesTests
{
    private static readonly Difference _difference =
        new("downloads", DifferenceKinds.NumberEquivalent, "LA", "UPN", "LA/201/pp2425_201_1.csv", "\"007\"", "\"7\"");

    [Theory]
    [InlineData(null, null, null, null)]
    [InlineData("*", "*", "*", null)]
    [InlineData("downloads", "la", "UPN", "\"0+\\d+\"")]
    public void FindRule_MatchingRule_ReturnsIt(string? comparison, string? dataset, string? field, string? newValue)
    {
        AcceptedDifferenceRule rule = Rule(comparison: comparison, dataset: dataset, field: field, newValue: newValue);

        Assert.Same(rule, new AcceptedDifferences([rule]).FindRule(_difference));
    }

    [Theory]
    [InlineData(DifferenceKinds.DateEquivalent, null, null, null)]
    [InlineData(DifferenceKinds.NumberEquivalent, "pupil-noskill", null, null)]
    [InlineData(DifferenceKinds.NumberEquivalent, null, "DOB", null)]
    // The pattern must match the whole value
    [InlineData(DifferenceKinds.NumberEquivalent, null, null, "0")]
    public void FindRule_NoMatchingRule_ReturnsNull(string kind, string? comparison, string? field, string? newValue)
    {
        AcceptedDifferences rules = new([Rule(kind, comparison, field: field, newValue: newValue)]);

        Assert.Null(rules.FindRule(_difference));
    }

    [Fact]
    public void Constructor_UnknownKind_Throws() =>
        Assert.Throws<ArgumentException>(() => new AcceptedDifferences([Rule("no-such-kind")]));

    [Fact]
    public void Constructor_NoReason_Throws() =>
        Assert.Throws<ArgumentException>(() => new AcceptedDifferences([Rule(reason: " ")]));

    [Fact]
    public void Load_ShippedRules_AreValid()
    {
        AcceptedDifferences rules = AcceptedDifferences.Load(Path.Combine(AppContext.BaseDirectory, AcceptedDifferences.DefaultFileName));

        Assert.All(rules.Rules, r => Assert.StartsWith("docs/", r.Docs, StringComparison.Ordinal));
    }

    private static AcceptedDifferenceRule Rule(
        string kind = DifferenceKinds.NumberEquivalent,
        string? comparison = null,
        string? dataset = null,
        string? field = null,
        string? newValue = null,
        string reason = "Spark inferred the type",
        string docs = "docs/file-ingestion.md#prepared-downloads") =>
        new(kind, comparison, dataset, field, newValue, OldValue: null, reason, docs);
}
