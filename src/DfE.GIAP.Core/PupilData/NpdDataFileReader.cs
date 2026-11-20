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
/// Reads the rows of an NPD data file, rejecting the rows that can't be written.
/// </summary>
/// <remarks>
/// Matches the old pipeline's reading and quality checks. Files are always tab-separated UTF-8, whatever their
/// extension. Values are trimmed, and an empty value is null; anything else, including <c>NULL</c>, is kept as it is.
/// A row is rejected if its PupilMatchingRef is empty, its DOB isn't a valid date or its academic year isn't the
/// file's. A row with no academic year is given the file's, but its <see cref="NpdFileRow.Data"/> is left as it is.
/// </remarks>
public sealed class NpdDataFileReader : IDisposable
{
    private const string EntryDateColumn = "EntryDate";

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
    private readonly string _academicYear;
    private readonly string[] _header;
    private readonly string[] _jsonNames;
    private readonly ColumnIndexes _columns;
    private readonly int? _entryDateIndex;
    private readonly List<IngestRowRejection> _rejectedRows = [];

    private NpdDataFileReader(
        StreamReader streamReader,
        CsvParser parser,
        NpdDatasetDefinition definition,
        string academicYear,
        string[] header)
    {
        _streamReader = streamReader;
        _parser = parser;
        _academicYear = academicYear;
        _header = header;
        // As the old pipeline did, since Spark doesn't allow dots in column names
        _jsonNames = [.. header.Select(c => c.Replace('.', '_'))];

        string[] duplicates = [.. _jsonNames.GroupBy(c => c, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1).Select(g => g.Key)];
        if (duplicates.Length > 0)
        {
            throw new IngestDataFileException(
                $"The data file has more than one of the following columns: {string.Join(", ", duplicates.Select(c => $"'{c}'"))}.");
        }

        string[] missingColumns = [.. definition.Columns.Where(c => IndexOf(c) is null)];
        if (missingColumns.Length > 0)
        {
            throw new IngestDataFileException(
                $"The data file is missing the following columns: {string.Join(", ", missingColumns.Select(c => $"'{c}'"))}.");
        }

        _columns = new ColumnIndexes(
            new Column(definition.PupilMatchingRef, IndexOf(definition.PupilMatchingRef)!.Value),
            IndexOf(definition.LaCode)!.Value,
            IndexOf(definition.EstablishmentNumber)!.Value,
            IndexOf(definition.Urn)!.Value,
            IndexOf(definition.Upn)!.Value,
            IndexOf(definition.Surname)!.Value,
            IndexOf(definition.Forename)!.Value,
            definition.MiddleNames is null ? null : IndexOf(definition.MiddleNames)!.Value,
            IndexOf(definition.Sex)!.Value,
            new Column(definition.Dob, IndexOf(definition.Dob)!.Value),
            new Column(definition.AcademicYear, IndexOf(definition.AcademicYear)!.Value));
        _entryDateIndex = IndexOf(EntryDateColumn);
    }

    /// <summary>
    /// The number of rows read so far, including rejected rows.
    /// </summary>
    public int RowsRead { get; private set; }

    public IReadOnlyList<IngestRowRejection> RejectedRows => _rejectedRows;

    /// <summary>
    /// Reads the file's header.
    /// </summary>
    /// <param name="academicYear">The academic year from the file's name, e.g. <c>2024/2025</c>.</param>
    /// <exception cref="IngestDataFileException">The file is empty, or its header is missing a column.</exception>
    public static async Task<NpdDataFileReader> OpenAsync(
        Stream content,
        NpdDatasetDefinition definition,
        string academicYear,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentException.ThrowIfNullOrEmpty(academicYear);

        StreamReader streamReader = new(content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), detectEncodingFromByteOrderMarks: true);
        CsvParser parser = new(streamReader, _csvConfiguration);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!await parser.ReadAsync())
            {
                throw new IngestDataFileException("The data file is empty.");
            }

            return new NpdDataFileReader(streamReader, parser, definition, academicYear, [.. parser.Record!.Select(c => c.Trim())]);
        }
        catch
        {
            parser.Dispose();
            streamReader.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Reads the rows that can be written; the others are added to <see cref="RejectedRows"/>.
    /// </summary>
    public async IAsyncEnumerable<NpdFileRow> ReadRowsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (await _parser.ReadAsync())
        {
            cancellationToken.ThrowIfCancellationRequested();

            RowsRead++;
            string?[] values = [.. Enumerable.Range(0, _header.Length).Select(i => i < _parser.Count ? NullIfEmpty(_parser[i]) : null)];

            if (ReadRow(RowsRead, values) is { } row)
            {
                yield return row;
            }
        }
    }

    public void Dispose()
    {
        _parser.Dispose();
        _streamReader.Dispose();
    }

    private NpdFileRow? ReadRow(int rowNumber, string?[] values)
    {
        List<string> reasons = [];

        string? pupilMatchingRef = values[_columns.PupilMatchingRef.Index];
        if (pupilMatchingRef is null)
        {
            reasons.Add($"{_columns.PupilMatchingRef.Name} is missing.");
        }

        string? dobValue = values[_columns.Dob.Index];
        DateOnly? dob = null;
        if (dobValue is not null)
        {
            dob = ParseDate(dobValue);

            if (dob is null)
            {
                reasons.Add($"{_columns.Dob.Name} isn't a valid date: '{dobValue}'.");
            }
        }

        string? academicYear = values[_columns.AcademicYear.Index];
        if (academicYear is not null && academicYear != _academicYear)
        {
            reasons.Add($"{_columns.AcademicYear.Name} '{academicYear}' doesn't match the file's academic year, {_academicYear}.");
        }

        if (reasons.Count > 0)
        {
            _rejectedRows.Add(new IngestRowRejection(
                rowNumber,
                _header.Select((c, i) => KeyValuePair.Create(c, values[i])).ToDictionary(),
                reasons));

            return null;
        }

        return new NpdFileRow(
            rowNumber,
            pupilMatchingRef!,
            Upn: values[_columns.Upn],
            LaCode: ParseInt(values[_columns.LaCode]),
            EstablishmentNumber: ParseInt(values[_columns.EstablishmentNumber]),
            Urn: ParseInt(values[_columns.Urn]),
            Surname: values[_columns.Surname],
            Forename: values[_columns.Forename],
            MiddleNames: _columns.MiddleNames is { } middleNames ? values[middleNames] : null,
            Sex: values[_columns.Sex],
            Dob: dob,
            Data: ToJson(values, dob));
    }

    private string ToJson(string?[] values, DateOnly? dob)
    {
        ArrayBufferWriter<byte> buffer = new();

        using (Utf8JsonWriter writer = new(buffer))
        {
            writer.WriteStartObject();

            for (int i = 0; i < values.Length; i++)
            {
                string? value = values[i];

                if (i == _columns.Dob.Index)
                {
                    value = dob is { } date ? FormatDate(date) : null;
                }
                else if (i == _entryDateIndex && value is not null)
                {
                    value = ParseDate(value) is { } date ? FormatDate(date) : null;
                }

                writer.WriteString(_jsonNames[i], value);
            }

            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(buffer.WrittenSpan);
    }

    private int? IndexOf(string column)
    {
        int index = Array.FindIndex(_header, c => string.Equals(c, column, StringComparison.OrdinalIgnoreCase));
        return index < 0 ? null : index;
    }

    private static string? NullIfEmpty(string value)
    {
        string trimmed = value.Trim();
        return trimmed.Length == 0 ? null : trimmed;
    }

    private static int? ParseInt(string? value) =>
        int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result) ? result : null;

    private static DateOnly? ParseDate(string value) =>
        DateTime.TryParseExact(value, _dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result)
            ? DateOnly.FromDateTime(result)
            : null;

    // The format the old pipeline wrote dates in
    private static string FormatDate(DateOnly date) =>
        date.ToString("yyyy-MM-dd'T00:00:00.000Z'", CultureInfo.InvariantCulture);

    private readonly record struct Column(string Name, int Index);

    private sealed record ColumnIndexes(
        Column PupilMatchingRef,
        int LaCode,
        int EstablishmentNumber,
        int Urn,
        int Upn,
        int Surname,
        int Forename,
        int? MiddleNames,
        int Sex,
        Column Dob,
        Column AcademicYear);
}
