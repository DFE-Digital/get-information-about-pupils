using System.Text.Json.Nodes;
using DfE.GIAP.Verification.Comparison;

namespace DfE.GIAP.Verification.Tests.Comparison;

public class ValueComparerTests
{
    [Theory]
    [InlineData("\"Smith\"", "\"Smith\"")]
    [InlineData("7", "7")]
    [InlineData("null", "null")]
    [InlineData(null, "null")]
    public void Compare_EqualValues_ReturnsNull(string? newJson, string? oldJson) =>
        Assert.Null(ValueComparer.Compare(Parse(newJson), Parse(oldJson)));

    [Theory]
    // Spark inferred numbers, which lose leading zeros and trailing decimal zeros
    [InlineData("\"007\"", "7", DifferenceKinds.NumberEquivalent)]
    [InlineData("\"1.50\"", "1.5", DifferenceKinds.NumberEquivalent)]
    [InlineData("\"1\"", "\"1.0\"", DifferenceKinds.NumberEquivalent)]
    [InlineData("\"true\"", "true", DifferenceKinds.BooleanEquivalent)]
    [InlineData("\"2010-03-01\"", "\"2010-03-01T00:00:00.000Z\"", DifferenceKinds.DateEquivalent)]
    // Slashed dates are day-first
    [InlineData("\"01/03/2010\"", "\"2010-03-01T00:00:00.000Z\"", DifferenceKinds.DateEquivalent)]
    public void Compare_EquivalentValues_ReturnsTheKindOfEquivalence(string newJson, string oldJson, string expected) =>
        Assert.Equal(expected, ValueComparer.Compare(Parse(newJson), Parse(oldJson)));

    [Theory]
    [InlineData("\"Smith\"", "\"Jones\"")]
    [InlineData("\"007\"", "8")]
    [InlineData("\"7\"", "null")]
    [InlineData(null, "\"Smith\"")]
    // A date read month-first rather than day-first
    [InlineData("\"01/03/2010\"", "\"2010-01-03T00:00:00.000Z\"")]
    [InlineData("\"1\"", "true")]
    [InlineData("\"Smith\"", "[\"Smith\"]")]
    public void Compare_DifferentValues_ReturnsValueDifferent(string? newJson, string oldJson) =>
        Assert.Equal(DifferenceKinds.ValueDifferent, ValueComparer.Compare(Parse(newJson), Parse(oldJson)));

    [Fact]
    public void Canonicalise_EquivalentValues_AreTheSame()
    {
        Assert.Equal(ValueComparer.Canonicalise(Parse("\"007\"")), ValueComparer.Canonicalise(Parse("7")));
        Assert.Equal(ValueComparer.Canonicalise(Parse("\"1.50\"")), ValueComparer.Canonicalise(Parse("1.5")));
        Assert.Equal(
            ValueComparer.Canonicalise(Parse("\"2010-03-01\"")),
            ValueComparer.Canonicalise(Parse("\"2010-03-01T00:00:00.000Z\"")));
        Assert.Null(ValueComparer.Canonicalise(Parse("null")));
        Assert.NotEqual(ValueComparer.Canonicalise(Parse("\"7\"")), ValueComparer.Canonicalise(Parse("\"8\"")));
    }

    private static JsonNode? Parse(string? json) => json is null ? null : JsonNode.Parse(json);
}
