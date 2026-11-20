using System.Runtime.CompilerServices;
using DfE.GIAP.Core.Ingest;

namespace DfE.GIAP.Core.PupilData;

/// <summary>
/// Reads the rows of a further education (pupil premium or SEN) data file, rejecting the rows that can't be written.
/// </summary>
/// <remarks>
/// Matches the old pipeline's reading and quality checks; see <see cref="DataFileReader"/> for how the file is read.
/// A row is rejected if its ULN is empty, its DOB isn't a valid date or its academic year isn't the file's, written
/// as e.g. <c>2025/26</c> or <c>2025/2026</c>. A row with no academic year is given the file's. A row's
/// <see cref="FurtherEducationFileRow.Data"/> keeps its academic year as it is.
/// </remarks>
public sealed class FurtherEducationDataFileReader : IDisposable
{
    private const string UlnColumn = "ULN";
    private const string SurnameColumn = "Surname";
    private const string ForenameColumn = "Forename";
    private const string SexColumn = "Sex";
    private const string DobColumn = "DOB";
    private const string AcademicYearColumn = "ACAD_YEAR";

    // The old pipeline's fe_pp_header_mapping and fe_sen_header_mapping; the former has Sex misspelt as Sec
    private static readonly string[] _requiredColumns =
        [UlnColumn, SurnameColumn, ForenameColumn, SexColumn, DobColumn, AcademicYearColumn];

    private readonly DataFileReader _reader;
    private readonly string _academicYear;

    // FE files write academic years as e.g. 2025/26
    private readonly string _shortAcademicYear;
    private readonly int _uln;
    private readonly int _surname;
    private readonly int _forename;
    private readonly int _sex;
    private readonly int _dob;
    private readonly int _academicYearIndex;

    private FurtherEducationDataFileReader(DataFileReader reader, string academicYear)
    {
        _reader = reader;
        _academicYear = academicYear;
        _shortAcademicYear = $"{academicYear[..5]}{academicYear[^2..]}";
        _uln = reader.GetColumn(UlnColumn);
        _surname = reader.GetColumn(SurnameColumn);
        _forename = reader.GetColumn(ForenameColumn);
        _sex = reader.GetColumn(SexColumn);
        _dob = reader.GetColumn(DobColumn);
        _academicYearIndex = reader.GetColumn(AcademicYearColumn);
    }

    /// <inheritdoc cref="DataFileReader.RowsRead"/>
    public int RowsRead => _reader.RowsRead;

    public IReadOnlyList<IngestRowRejection> RejectedRows => _reader.RejectedRows;

    /// <summary>
    /// Reads the file's header.
    /// </summary>
    /// <param name="academicYear">The academic year from the file's name, e.g. <c>2024/2025</c>.</param>
    /// <exception cref="IngestDataFileException">The file is empty, or its header is missing a column.</exception>
    public static async Task<FurtherEducationDataFileReader> OpenAsync(
        Stream content,
        string academicYear,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrEmpty(academicYear);

        DataFileReader reader = await DataFileReader.OpenAsync(content, _requiredColumns, cancellationToken);

        return new FurtherEducationDataFileReader(reader, academicYear);
    }

    /// <summary>
    /// Reads the rows that can be written; the others are added to <see cref="RejectedRows"/>.
    /// </summary>
    public async IAsyncEnumerable<FurtherEducationFileRow> ReadRowsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (DataFileRow row in _reader.ReadRowsAsync(cancellationToken))
        {
            if (ReadRow(row) is { } furtherEducationRow)
            {
                yield return furtherEducationRow;
            }
        }
    }

    public void Dispose() => _reader.Dispose();

    private FurtherEducationFileRow? ReadRow(DataFileRow row)
    {
        string?[] values = row.Values;
        List<string> reasons = [];

        string? uln = values[_uln];
        if (uln is null)
        {
            reasons.Add($"{_reader.Header[_uln]} is missing.");
        }

        string? dobValue = values[_dob];
        DateOnly? dob = null;
        if (dobValue is not null)
        {
            dob = DataFileReader.ParseDate(dobValue);

            if (dob is null)
            {
                reasons.Add($"{_reader.Header[_dob]} isn't a valid date: '{dobValue}'.");
            }
        }

        string? academicYear = values[_academicYearIndex];
        if (academicYear is not null && academicYear != _shortAcademicYear && academicYear != _academicYear)
        {
            reasons.Add($"{_reader.Header[_academicYearIndex]} '{academicYear}' doesn't match the file's academic year, {_academicYear}.");
        }

        if (reasons.Count > 0)
        {
            _reader.Reject(row, reasons);
            return null;
        }

        string?[] json = [.. values];
        json[_dob] = dob is { } date ? DataFileReader.FormatDate(date) : null;

        return new FurtherEducationFileRow(
            row.RowNumber,
            uln!,
            Surname: values[_surname],
            Forename: values[_forename],
            Sex: values[_sex],
            Dob: dob,
            Data: DataFileReader.ToJson(_reader.JsonNames, json));
    }
}
