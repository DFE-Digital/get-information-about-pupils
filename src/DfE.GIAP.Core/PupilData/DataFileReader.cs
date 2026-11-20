using System.Buffers;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;
using DfE.GIAP.Core.Ingest;

namespace DfE.GIAP.Core.PupilData;

/// <summary>
/// Reads the rows of a pupil data file, and records the rows the caller rejects.
/// </summary>
/// <remarks>
/// Matches the old pipeline's reading. Files are always tab-separated UTF-8, whatever their extension. Column names
/// are matched case-insensitively. Values are trimmed, and an empty value is null; anything else, including
/// <c>NULL</c>, is kept as it is.
/// </remarks>
public sealed class DataFileReader : IDisposable
{
    private static readonly CsvConfiguration _csvConfiguration = new(CultureInfo.InvariantCulture)
    {
        Delimiter = "\t",
        // The old pipeline's Spark reader didn't fail on these, so neither do we
        BadDataFound = null,
        MissingFieldFound = null
    };

    // The old pipeline parsed slashed dates month-first; NPD dates are day-first
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

    private readonly StreamReader _streamReader;
    private readonly CsvParser _parser;
    private readonly string[] _header;
    private readonly List<IngestRowRejection> _rejectedRows = [];

    private DataFileReader(StreamReader streamReader, CsvParser parser, string[] header, IEnumerable<string> requiredColumns)
    {
        _streamReader = streamReader;
        _parser = parser;
        _header = header;
        // As the old pipeline did, since Spark doesn't allow dots in column names
        JsonNames = [.. header.Select(c => c.Replace('.', '_'))];

        string[] duplicates = [.. JsonNames.GroupBy(c => c, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1).Select(g => g.Key)];
        if (duplicates.Length > 0)
        {
            throw new IngestDataFileException(
                $"The data file has more than one of the following columns: {string.Join(", ", duplicates.Select(c => $"'{c}'"))}.");
        }

        string[] missingColumns = [.. requiredColumns.Where(c => FindColumn(c) is null)];
        if (missingColumns.Length > 0)
        {
            throw new IngestDataFileException(
                $"The data file is missing the following columns: {string.Join(", ", missingColumns.Select(c => $"'{c}'"))}.");
        }
    }

    /// <summary>
    /// The column names, trimmed.
    /// </summary>
    public IReadOnlyList<string> Header => _header;

    /// <summary>
    /// The names the columns are written to JSON with.
    /// </summary>
    public IReadOnlyList<string> JsonNames { get; }

    /// <summary>
    /// The number of rows read so far, including rejected rows.
    /// </summary>
    public int RowsRead { get; private set; }

    public IReadOnlyList<IngestRowRejection> RejectedRows => _rejectedRows;

    /// <summary>
    /// Reads the file's header.
    /// </summary>
    /// <exception cref="IngestDataFileException">
    /// The file is empty, its header is missing one of <paramref name="requiredColumns"/>, or two of its columns have
    /// the same name.
    /// </exception>
    public static async Task<DataFileReader> OpenAsync(
        Stream content,
        IEnumerable<string> requiredColumns,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(requiredColumns);

        StreamReader streamReader = new(content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), detectEncodingFromByteOrderMarks: true);
        CsvParser parser = new(streamReader, _csvConfiguration);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!await parser.ReadAsync())
            {
                throw new IngestDataFileException("The data file is empty.");
            }

            return new DataFileReader(streamReader, parser, [.. parser.Record!.Select(c => c.Trim())], requiredColumns);
        }
        catch
        {
            parser.Dispose();
            streamReader.Dispose();
            throw;
        }
    }

    /// <summary>
    /// The index of a column in <see cref="Header"/>, or <see langword="null"/> if the file doesn't have it.
    /// </summary>
    public int? FindColumn(string column)
    {
        int index = Array.FindIndex(_header, c => string.Equals(c, column, StringComparison.OrdinalIgnoreCase));
        return index < 0 ? null : index;
    }

    /// <summary>
    /// The index of one of the required columns.
    /// </summary>
    public int GetColumn(string column) =>
        FindColumn(column) ?? throw new ArgumentException($"The data file has no column named '{column}'.", nameof(column));

    /// <summary>
    /// Reads every row; each one has a value for every column in <see cref="Header"/>.
    /// </summary>
    public async IAsyncEnumerable<DataFileRow> ReadRowsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (await _parser.ReadAsync())
        {
            cancellationToken.ThrowIfCancellationRequested();

            RowsRead++;
            string?[] values = [.. Enumerable.Range(0, _header.Length).Select(i => i < _parser.Count ? NullIfEmpty(_parser[i]) : null)];

            yield return new DataFileRow(RowsRead, values);
        }
    }

    public void Reject(DataFileRow row, IReadOnlyList<string> reasons)
    {
        ArgumentNullException.ThrowIfNull(row);
        ArgumentNullException.ThrowIfNull(reasons);

        _rejectedRows.Add(new IngestRowRejection(
            row.RowNumber,
            _header.Select((c, i) => KeyValuePair.Create(c, row.Values[i])).ToDictionary(),
            reasons));
    }

    public void Dispose()
    {
        _parser.Dispose();
        _streamReader.Dispose();
    }

    /// <summary>
    /// Writes a row's values as a JSON object.
    /// </summary>
    /// <param name="names">The name of each value.</param>
    public static string ToJson(IReadOnlyList<string> names, IReadOnlyList<string?> values)
    {
        ArgumentNullException.ThrowIfNull(names);
        ArgumentNullException.ThrowIfNull(values);

        ArrayBufferWriter<byte> buffer = new();

        using (Utf8JsonWriter writer = new(buffer))
        {
            writer.WriteStartObject();

            for (int i = 0; i < values.Count; i++)
            {
                writer.WriteString(names[i], values[i]);
            }

            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(buffer.WrittenSpan);
    }

    public static int? ParseInt(string? value) =>
        int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result) ? result : null;

    public static DateOnly? ParseDate(string value) =>
        DateTime.TryParseExact(value, _dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result)
            ? DateOnly.FromDateTime(result)
            : null;

    // The format the old pipeline wrote dates in
    public static string FormatDate(DateOnly date) =>
        date.ToString("yyyy-MM-dd'T00:00:00.000Z'", CultureInfo.InvariantCulture);

    private static string? NullIfEmpty(string value)
    {
        string trimmed = value.Trim();
        return trimmed.Length == 0 ? null : trimmed;
    }
}

/// <param name="RowNumber">The row's 1-based position in the file, not counting the header.</param>
/// <param name="Values">The row's values, in the order of <see cref="DataFileReader.Header"/>.</param>
public sealed record DataFileRow(int RowNumber, string?[] Values);
