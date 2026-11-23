using System.Globalization;
using System.Text;
using System.Text.Json.Nodes;
using CsvHelper;

namespace DfE.GIAP.Verification.Comparison;

/// <summary>
/// Compares a prepared download written by each pipeline: its encoding, header and rows.
/// </summary>
public static class DownloadComparer
{
    public const string Comparison = "downloads";

    private static readonly byte[] _bom = [0xEF, 0xBB, 0xBF];

    // Columns that identify a pupil in each kind of prepared download, used to pair up rows that differ
    private static readonly string[] _identityColumns = ["UPN", "CLA_UPN"];

    /// <param name="path">The file's path in the container, e.g. <c>LA/201/pp2425_201_1.csv</c>.</param>
    public static IEnumerable<Difference> Compare(string path, byte[] newContent, byte[] oldContent)
    {
        ArgumentNullException.ThrowIfNull(newContent);
        ArgumentNullException.ThrowIfNull(oldContent);

        string dataset = path.Split('/')[0];

        bool newBom = newContent.AsSpan().StartsWith(_bom);
        bool oldBom = oldContent.AsSpan().StartsWith(_bom);
        if (newBom != oldBom)
        {
            yield return new Difference(Comparison, DifferenceKinds.BomDifferent, dataset, Field: null, path, Json(newBom), Json(oldBom));
        }

        (string newText, bool newUtf8) = Decode(newContent, newBom);
        (string oldText, bool oldUtf8) = Decode(oldContent, oldBom);
        if (newUtf8 != oldUtf8 || !newUtf8)
        {
            yield return new Difference(Comparison, DifferenceKinds.EncodingDifferent, dataset, Field: null, path, Encoding(newUtf8), Encoding(oldUtf8));
        }

        string newLineEndings = LineEndings(newText);
        string oldLineEndings = LineEndings(oldText);
        if (newLineEndings != oldLineEndings)
        {
            yield return new Difference(Comparison, DifferenceKinds.LineEndingsDifferent, dataset, Field: null, path, Json(newLineEndings), Json(oldLineEndings));
        }

        (string[] newHeader, List<JsonObject> newRows) = Parse(newText);
        (string[] oldHeader, List<JsonObject> oldRows) = Parse(oldText);
        if (!newHeader.SequenceEqual(oldHeader, StringComparer.Ordinal))
        {
            yield return new Difference(
                Comparison,
                DifferenceKinds.HeaderDifferent,
                dataset,
                Field: null,
                path,
                new JsonArray([.. newHeader.Select(h => JsonValue.Create(h))]).ToJsonString(),
                new JsonArray([.. oldHeader.Select(h => JsonValue.Create(h))]).ToJsonString());
        }

        string? identityColumn = _identityColumns.FirstOrDefault(c => newHeader.Contains(c) && oldHeader.Contains(c));

        foreach (Difference difference in RecordComparer.CompareRows(Comparison, path, dataset, newRows, oldRows, identityColumn))
        {
            yield return difference;
        }
    }

    private static (string Text, bool IsUtf8) Decode(byte[] content, bool hasBom)
    {
        ReadOnlySpan<byte> bytes = hasBom ? content.AsSpan(_bom.Length) : content;

        try
        {
            return (new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true).GetString(bytes), true);
        }
        catch (DecoderFallbackException)
        {
            return (System.Text.Encoding.Latin1.GetString(bytes), false);
        }
    }

    private static string LineEndings(string text) =>
        text.Contains("\r\n", StringComparison.Ordinal) ? "CRLF" : text.Contains('\n', StringComparison.Ordinal) ? "LF" : "none";

    private static (string[] Header, List<JsonObject> Rows) Parse(string text)
    {
        using StringReader reader = new(text);
        using CsvReader csv = new(reader, CultureInfo.InvariantCulture);

        if (!csv.Read() || !csv.ReadHeader() || csv.HeaderRecord is not { } header)
        {
            return ([], []);
        }

        // A column name that's repeated is numbered, so no value is lost
        string[] names = [.. header.Select((h, i) => header.Take(i).Contains(h) ? $"{h}#{header.Take(i).Count(p => p == h) + 1}" : h)];

        List<JsonObject> rows = [];
        while (csv.Read())
        {
            JsonObject row = [];
            for (int i = 0; i < names.Length; i++)
            {
                row[names[i]] = csv.TryGetField(i, out string? value) && value is { Length: > 0 } ? value : null;
            }

            rows.Add(row);
        }

        return (header, rows);
    }

    private static string Json(bool value) => value ? "true" : "false";

    private static string Json(string value) => JsonValue.Create(value).ToJsonString();

    private static string Encoding(bool isUtf8) => Json(isUtf8 ? "UTF-8" : "not UTF-8");
}
