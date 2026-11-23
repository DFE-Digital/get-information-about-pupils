using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace DfE.GIAP.Verification.Comparison;

/// <summary>
/// Compares a value written by the new pipeline with one written by the old pipeline.
/// </summary>
/// <remarks>
/// The new pipeline keeps values as the strings in the data file, while the old pipeline wrote the types Spark inferred
/// for them. A value that's only different in that way, e.g. <c>"007"</c> and <c>7</c>, is equivalent rather than
/// different, so that it can be accepted without hiding a value that's actually wrong.
/// </remarks>
public static partial class ValueComparer
{
    private static readonly string[] _dateFormats =
    [
        "yyyy-MM-dd",
        "yyyy-MM-dd HH:mm:ss",
        "yyyy-MM-dd HH:mm:ss.FFFFFFF",
        "yyyy-MM-dd'T'HH:mm:ss",
        "yyyy-MM-dd'T'HH:mm:ss.FFFFFFF",
        "yyyy-MM-dd'T'HH:mm:ss'Z'",
        "yyyy-MM-dd'T'HH:mm:ss.FFFFFFF'Z'",
        "d'/'M'/'yyyy",
        "d'/'M'/'yyyy HH:mm",
        "d'/'M'/'yyyy HH:mm:ss"
    ];

    /// <returns>
    /// <see langword="null"/> if the values are equal, otherwise the <see cref="DifferenceKinds">kind</see> of
    /// difference. A missing value is the same as a JSON <c>null</c>.
    /// </returns>
    public static string? Compare(JsonNode? newValue, JsonNode? oldValue)
    {
        newValue = WithoutNull(newValue);
        oldValue = WithoutNull(oldValue);

        if (newValue is null && oldValue is null)
        {
            return null;
        }

        if (newValue is null || oldValue is null)
        {
            return DifferenceKinds.ValueDifferent;
        }

        if (JsonNode.DeepEquals(newValue, oldValue))
        {
            return null;
        }

        if (newValue is not JsonValue newScalar || oldValue is not JsonValue oldScalar)
        {
            return DifferenceKinds.ValueDifferent;
        }

        if (TryGetNumber(newScalar, out decimal newNumber) && TryGetNumber(oldScalar, out decimal oldNumber))
        {
            return newNumber == oldNumber ? DifferenceKinds.NumberEquivalent : DifferenceKinds.ValueDifferent;
        }

        if (TryGetBoolean(newScalar, out bool newBoolean) && TryGetBoolean(oldScalar, out bool oldBoolean))
        {
            return newBoolean == oldBoolean ? DifferenceKinds.BooleanEquivalent : DifferenceKinds.ValueDifferent;
        }

        if (TryGetDate(newScalar, out DateTime newDate) && TryGetDate(oldScalar, out DateTime oldDate))
        {
            return newDate == oldDate ? DifferenceKinds.DateEquivalent : DifferenceKinds.ValueDifferent;
        }

        return DifferenceKinds.ValueDifferent;
    }

    /// <summary>
    /// A form of the value that's the same for equal and equivalent values; <see langword="null"/> for a missing or
    /// null value.
    /// </summary>
    public static string? Canonicalise(JsonNode? value)
    {
        value = WithoutNull(value);

        if (value is not JsonValue scalar)
        {
            return value?.ToJsonString();
        }

        if (TryGetNumber(scalar, out decimal number))
        {
            // G29 drops trailing zeros, so 1.50 and 1.5 are the same
            return "n:" + number.ToString("G29", CultureInfo.InvariantCulture);
        }

        if (TryGetBoolean(scalar, out bool boolean))
        {
            return boolean ? "b:true" : "b:false";
        }

        if (TryGetDate(scalar, out DateTime date))
        {
            return "d:" + date.ToString("O", CultureInfo.InvariantCulture);
        }

        return "s:" + scalar.ToJsonString();
    }

    private static JsonNode? WithoutNull(JsonNode? value) =>
        value is JsonValue scalar && scalar.GetValueKind() == JsonValueKind.Null ? null : value;

    private static bool TryGetNumber(JsonValue value, out decimal number)
    {
        number = 0;

        return value.GetValueKind() switch
        {
            JsonValueKind.Number => decimal.TryParse(value.ToJsonString(), NumberStyles.Float, CultureInfo.InvariantCulture, out number),
            JsonValueKind.String => value.GetValue<string>() is { } text
                && NumberPattern().IsMatch(text)
                && decimal.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out number),
            _ => false
        };
    }

    private static bool TryGetBoolean(JsonValue value, out bool boolean)
    {
        boolean = value.GetValueKind() == JsonValueKind.True;

        return value.GetValueKind() switch
        {
            JsonValueKind.True or JsonValueKind.False => true,
            JsonValueKind.String => bool.TryParse(value.GetValue<string>(), out boolean),
            _ => false
        };
    }

    private static bool TryGetDate(JsonValue value, out DateTime date)
    {
        date = default;

        return value.GetValueKind() == JsonValueKind.String
            && DateTime.TryParseExact(
                value.GetValue<string>(),
                _dateFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                out date);
    }

    [GeneratedRegex(@"^[+-]?(\d+(\.\d*)?|\.\d+)([eE][+-]?\d+)?$")]
    private static partial Regex NumberPattern();
}
