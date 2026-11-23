namespace DfE.GIAP.Verification.Comparison;

/// <summary>
/// A difference between what the new pipeline wrote (to Postgres and its own storage) and what the old pipeline wrote
/// (to Cosmos DB and its storage accounts).
/// </summary>
/// <param name="Comparison">What was being compared, e.g. <c>pupil-noskill</c> or <c>downloads</c>.</param>
/// <param name="Dataset">E.g. <c>KS2</c>, or <see langword="null"/> for the record as a whole.</param>
/// <param name="Field">The field or column, or <see langword="null"/> for a whole record or row.</param>
/// <param name="Key">What identifies the record: a PupilMatchingRef, ULN, data file name or blob path.</param>
/// <param name="NewValue">The new pipeline's value as JSON, if there is one.</param>
/// <param name="OldValue">The old pipeline's value as JSON, if there is one.</param>
public sealed record Difference(
    string Comparison,
    string Kind,
    string? Dataset,
    string? Field,
    string Key,
    string? NewValue = null,
    string? OldValue = null);

/// <summary>
/// The kinds of <see cref="Difference"/>. They're the names used in the report and in accepted difference rules.
/// </summary>
public static class DifferenceKinds
{
    public const string MissingInNew = "missing-in-new";
    public const string MissingInOld = "missing-in-old";
    public const string FieldMissingInNew = "field-missing-in-new";
    public const string FieldMissingInOld = "field-missing-in-old";
    public const string RowMissingInNew = "row-missing-in-new";
    public const string RowMissingInOld = "row-missing-in-old";
    public const string RowOrder = "row-order";
    public const string ValueDifferent = "value-different";
    public const string NumberEquivalent = "number-equivalent";
    public const string DateEquivalent = "date-equivalent";
    public const string BooleanEquivalent = "boolean-equivalent";
    public const string HeaderDifferent = "header-different";
    public const string BomDifferent = "bom-different";
    public const string LineEndingsDifferent = "line-endings-different";
    public const string EncodingDifferent = "encoding-different";

    /// <summary>
    /// Something couldn't be compared, e.g. the old pipeline's rejected rows couldn't be found.
    /// </summary>
    public const string NotCompared = "not-compared";
}
