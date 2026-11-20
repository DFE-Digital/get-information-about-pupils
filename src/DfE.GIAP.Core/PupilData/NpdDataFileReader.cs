using System.Runtime.CompilerServices;
using DfE.GIAP.Core.Ingest;

namespace DfE.GIAP.Core.PupilData;

/// <summary>
/// Reads the rows of an NPD data file, rejecting the rows that can't be written.
/// </summary>
/// <remarks>
/// Matches the old pipeline's reading and quality checks; see <see cref="DataFileReader"/> for how the file is read.
/// A row is rejected if its PupilMatchingRef is empty, its DOB isn't a valid date or its academic year isn't the
/// file's. A row with no academic year is given the file's, but its <see cref="NpdFileRow.Data"/> is left as it is.
/// </remarks>
public sealed class NpdDataFileReader : IDisposable
{
    private const string EntryDateColumn = "EntryDate";

    private readonly DataFileReader _reader;
    private readonly string _academicYear;
    private readonly ColumnIndexes _columns;
    private readonly int? _entryDateIndex;

    private NpdDataFileReader(DataFileReader reader, NpdDatasetDefinition definition, string academicYear)
    {
        _reader = reader;
        _academicYear = academicYear;
        _columns = new ColumnIndexes(
            new Column(definition.PupilMatchingRef, reader.GetColumn(definition.PupilMatchingRef)),
            reader.GetColumn(definition.LaCode),
            reader.GetColumn(definition.EstablishmentNumber),
            reader.GetColumn(definition.Urn),
            reader.GetColumn(definition.Upn),
            reader.GetColumn(definition.Surname),
            reader.GetColumn(definition.Forename),
            definition.MiddleNames is null ? null : reader.GetColumn(definition.MiddleNames),
            reader.GetColumn(definition.Sex),
            new Column(definition.Dob, reader.GetColumn(definition.Dob)),
            new Column(definition.AcademicYear, reader.GetColumn(definition.AcademicYear)));
        _entryDateIndex = reader.FindColumn(EntryDateColumn);
    }

    /// <inheritdoc cref="DataFileReader.RowsRead"/>
    public int RowsRead => _reader.RowsRead;

    public IReadOnlyList<IngestRowRejection> RejectedRows => _reader.RejectedRows;

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

        DataFileReader reader = await DataFileReader.OpenAsync(content, definition.Columns, cancellationToken);

        return new NpdDataFileReader(reader, definition, academicYear);
    }

    /// <summary>
    /// Reads the rows that can be written; the others are added to <see cref="RejectedRows"/>.
    /// </summary>
    public async IAsyncEnumerable<NpdFileRow> ReadRowsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (DataFileRow row in _reader.ReadRowsAsync(cancellationToken))
        {
            if (ReadRow(row) is { } npdRow)
            {
                yield return npdRow;
            }
        }
    }

    public void Dispose() => _reader.Dispose();

    private NpdFileRow? ReadRow(DataFileRow row)
    {
        string?[] values = row.Values;
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
            dob = DataFileReader.ParseDate(dobValue);

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
            _reader.Reject(row, reasons);
            return null;
        }

        return new NpdFileRow(
            row.RowNumber,
            pupilMatchingRef!,
            Upn: values[_columns.Upn],
            LaCode: DataFileReader.ParseInt(values[_columns.LaCode]),
            EstablishmentNumber: DataFileReader.ParseInt(values[_columns.EstablishmentNumber]),
            Urn: DataFileReader.ParseInt(values[_columns.Urn]),
            Surname: values[_columns.Surname],
            Forename: values[_columns.Forename],
            MiddleNames: _columns.MiddleNames is { } middleNames ? values[middleNames] : null,
            Sex: values[_columns.Sex],
            Dob: dob,
            Data: ToJson(values, dob));
    }

    private string ToJson(string?[] values, DateOnly? dob)
    {
        string?[] json = [.. values];
        json[_columns.Dob.Index] = dob is { } date ? DataFileReader.FormatDate(date) : null;

        if (_entryDateIndex is { } entryDateIndex && json[entryDateIndex] is { } entryDate)
        {
            json[entryDateIndex] = DataFileReader.ParseDate(entryDate) is { } parsed ? DataFileReader.FormatDate(parsed) : null;
        }

        return DataFileReader.ToJson(_reader.JsonNames, json);
    }

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
