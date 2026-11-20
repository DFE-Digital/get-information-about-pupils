using System.Runtime.CompilerServices;
using DfE.GIAP.Core.Ingest;

namespace DfE.GIAP.Core.PupilData;

/// <summary>
/// Reads the rows of a pupil premium data file, rejecting the rows that can't be written.
/// </summary>
/// <remarks>
/// Matches the old pipeline's reading and quality checks; see <see cref="DataFileReader"/> for how the file is read.
/// A row is rejected if its PupilMatchingRef is empty, its DOB isn't a valid date or the pupil was adopted from care
/// (<c>Adopted</c> is <c>1</c>). In <see cref="PupilPremiumFileRow.Data"/>, <c>Adopted</c> is renamed to
/// <c>AdoptedfromCarePremium</c>, as the old pipeline did.
/// </remarks>
public sealed class PupilPremiumDataFileReader : IDisposable
{
    private const string PupilMatchingRefColumn = "PupilMatchingRef";
    private const string LaCodeColumn = "LA";
    private const string EstablishmentNumberColumn = "Estab";
    private const string UrnColumn = "URN";
    private const string UpnColumn = "UPN";
    private const string SurnameColumn = "Surname";
    private const string ForenameColumn = "Forename";
    private const string MiddleNamesColumn = "Middlenames";
    private const string SexColumn = "Sex";
    private const string DobColumn = "DOB";
    private const string SourceColumn = "Source";
    private const string AdoptedColumn = "Adopted";
    private const string AdoptedJsonName = "AdoptedfromCarePremium";

    // The old pipeline's pp_header_fields, and Adopted for its omit_adopted check
    private static readonly string[] _requiredColumns =
    [
        PupilMatchingRefColumn,
        LaCodeColumn,
        EstablishmentNumberColumn,
        UrnColumn,
        UpnColumn,
        SurnameColumn,
        ForenameColumn,
        MiddleNamesColumn,
        SexColumn,
        DobColumn,
        SourceColumn,
        AdoptedColumn
    ];

    private readonly DataFileReader _reader;
    private readonly string[] _jsonNames;
    private readonly int _pupilMatchingRef;
    private readonly int _laCode;
    private readonly int _establishmentNumber;
    private readonly int _urn;
    private readonly int _upn;
    private readonly int _surname;
    private readonly int _forename;
    private readonly int _middleNames;
    private readonly int _sex;
    private readonly int _dob;
    private readonly int _source;
    private readonly int _adopted;

    private PupilPremiumDataFileReader(DataFileReader reader)
    {
        _reader = reader;
        _pupilMatchingRef = reader.GetColumn(PupilMatchingRefColumn);
        _laCode = reader.GetColumn(LaCodeColumn);
        _establishmentNumber = reader.GetColumn(EstablishmentNumberColumn);
        _urn = reader.GetColumn(UrnColumn);
        _upn = reader.GetColumn(UpnColumn);
        _surname = reader.GetColumn(SurnameColumn);
        _forename = reader.GetColumn(ForenameColumn);
        _middleNames = reader.GetColumn(MiddleNamesColumn);
        _sex = reader.GetColumn(SexColumn);
        _dob = reader.GetColumn(DobColumn);
        _source = reader.GetColumn(SourceColumn);
        _adopted = reader.GetColumn(AdoptedColumn);

        _jsonNames = [.. reader.JsonNames];
        _jsonNames[_adopted] = AdoptedJsonName;
    }

    /// <inheritdoc cref="DataFileReader.RowsRead"/>
    public int RowsRead => _reader.RowsRead;

    public IReadOnlyList<IngestRowRejection> RejectedRows => _reader.RejectedRows;

    /// <summary>
    /// Reads the file's header.
    /// </summary>
    /// <exception cref="IngestDataFileException">The file is empty, or its header is missing a column.</exception>
    public static async Task<PupilPremiumDataFileReader> OpenAsync(Stream content, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        DataFileReader reader = await DataFileReader.OpenAsync(content, _requiredColumns, cancellationToken);

        return new PupilPremiumDataFileReader(reader);
    }

    /// <summary>
    /// Reads the rows that can be written; the others are added to <see cref="RejectedRows"/>.
    /// </summary>
    public async IAsyncEnumerable<PupilPremiumFileRow> ReadRowsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (DataFileRow row in _reader.ReadRowsAsync(cancellationToken))
        {
            if (ReadRow(row) is { } pupilPremiumRow)
            {
                yield return pupilPremiumRow;
            }
        }
    }

    public void Dispose() => _reader.Dispose();

    private PupilPremiumFileRow? ReadRow(DataFileRow row)
    {
        string?[] values = row.Values;
        List<string> reasons = [];

        string? pupilMatchingRef = values[_pupilMatchingRef];
        if (pupilMatchingRef is null)
        {
            reasons.Add($"{_reader.Header[_pupilMatchingRef]} is missing.");
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

        // The old pipeline's omit_adopted check
        if (values[_adopted] == "1")
        {
            reasons.Add($"{_reader.Header[_adopted]} is 1; pupils adopted from care aren't loaded.");
        }

        if (reasons.Count > 0)
        {
            _reader.Reject(row, reasons);
            return null;
        }

        string?[] json = [.. values];
        json[_dob] = dob is { } date ? DataFileReader.FormatDate(date) : null;

        return new PupilPremiumFileRow(
            row.RowNumber,
            pupilMatchingRef!,
            Upn: values[_upn],
            LaCode: DataFileReader.ParseInt(values[_laCode]),
            EstablishmentNumber: DataFileReader.ParseInt(values[_establishmentNumber]),
            Urn: DataFileReader.ParseInt(values[_urn]),
            Surname: values[_surname],
            Forename: values[_forename],
            MiddleNames: values[_middleNames],
            Sex: values[_sex],
            Dob: dob,
            Source: values[_source],
            Data: DataFileReader.ToJson(_jsonNames, json));
    }
}
