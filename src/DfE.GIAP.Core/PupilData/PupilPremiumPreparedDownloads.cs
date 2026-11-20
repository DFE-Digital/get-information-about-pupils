using System.Globalization;
using System.Runtime.CompilerServices;
using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using DfE.GIAP.Core.Ingest;

namespace DfE.GIAP.Core.PupilData;

/// <summary>
/// Writes the prepared downloads from a pupil premium data file: a file for each LA, school, MAT and SAT, and an
/// alternative provision file for each LA.
/// </summary>
/// <remarks>
/// Ported from the old pipeline's <c>create_partitions</c> notebook and <c>pp_selections</c>. Every row in the data
/// file is used, including the rows that aren't loaded into Postgres, e.g. pupils adopted from care.
/// </remarks>
public sealed class PupilPremiumPreparedDownloads(PreparedDownloadWriter writer)
{
    private const string LaColumn = "LA";
    private const string EstabColumn = "Estab";
    private const string UrnColumn = "URN";
    private const string LaEstabColumn = "LAEstab";
    private const string SourceColumn = "Source";
    private const string ModServiceColumn = "MODSERVICE";
    private const string CensusServiceEver6Column = "CENSUSSERVICEEVER6";
    private const string DobColumn = "DOB";
    private const string FyStartDateColumn = "PupilPremiumFYStartDate";
    private const string FyEndDateColumn = "PupilPremiumFYEndDate";

    // A rows are in the alternative provision files, and C rows in every other file
    private const string CensusSource = "C";
    private const string AlternativeProvisionSource = "A";

    private const int LaAudience = 0;
    private const int SchoolAudience = 1;
    private const int MatAudience = 2;
    private const int SatAudience = 3;
    private const int LaAlternativeProvisionAudience = 4;

    // The columns every file ends with, as (data file column, file column); Service Child Premium is derived
    private static readonly (string? Column, string Header)[] _pupilColumns =
    [
        ("UPN", "UPN"),
        ("Surname", "Surname"),
        ("Forename", "Forename"),
        ("Sex", "Sex"),
        (DobColumn, "DOB"),
        ("NCYear", "NCYear"),
        ("DeprivationPupilPremium", "Deprivation Pupil Premium"),
        (null, "Service Child Premium"),
        ("Adopted", "Adopted from Care Premium"),
        ("LookedAfterPremium", "Looked After Premium"),
        ("PupilPremiumFTE", "PupilPremiumFTE"),
        ("PupilPremiumCashAmount", "PupilPremiumCashAmount"),
        (FyStartDateColumn, "PupilPremiumFYStartDate"),
        (FyEndDateColumn, "PupilPremiumFYEndDate"),
        ("LastFSM", "LastFSM")
    ];

    private static readonly string[] _dateColumns = [DobColumn, FyStartDateColumn, FyEndDateColumn];

    private static readonly string[] _requiredColumns =
    [
        LaColumn,
        EstabColumn,
        UrnColumn,
        LaEstabColumn,
        SourceColumn,
        ModServiceColumn,
        CensusServiceEver6Column,
        .. _pupilColumns.Where(c => c.Column is not null).Select(c => c.Column!)
    ];

    /// <summary>
    /// Checks the data file has every column the prepared downloads need.
    /// </summary>
    /// <exception cref="IngestDataFileException">The file is empty, or its header is missing a column.</exception>
    public static async Task ValidateColumnsAsync(Stream content, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        using DataFileReader reader = await DataFileReader.OpenAsync(content, _requiredColumns, cancellationToken);
    }

    /// <returns>The number of files written.</returns>
    /// <exception cref="IngestDataFileException">The file is empty, or its header is missing a column.</exception>
    public async Task<int> WriteAsync(Stream content, IngestDataFileAttributes attributes, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(attributes);

        using DataFileReader reader = await DataFileReader.OpenAsync(content, _requiredColumns, cancellationToken);

        return await writer.WriteAsync(GetAudiences(attributes), ReadRowsAsync(reader, cancellationToken), cancellationToken);
    }

    // The old pipeline's file names, e.g. pp2425_201_1 for an LA's 2024/2025 unamended file; 2 is final
    private static PreparedDownloadAudience[] GetAudiences(IngestDataFileAttributes attributes)
    {
        string year = attributes.AcademicYear[2..4] + attributes.AcademicYear[7..9];

        string maturity = attributes.IngestVersion switch
        {
            IngestVersion.Unamended => "1",
            IngestVersion.Final => "2",
            _ => throw new ArgumentException($"A pupil premium file must be U or F, not {attributes.IngestVersion}.", nameof(attributes))
        };

        string fileName = $"pp{year}_*_{maturity}";
        string[] pupilHeader = [.. _pupilColumns.Select(c => c.Header)];

        PreparedDownloadAudience[] audiences = new PreparedDownloadAudience[5];
        audiences[LaAudience] = new("LA", "LA", fileName, ["LA", "Estab", .. pupilHeader]);
        audiences[SchoolAudience] = new("School", "School", fileName, pupilHeader);
        audiences[MatAudience] = new("MAT", "MAT", fileName, ["GroupUID", "LA", "Estab", .. pupilHeader], EstablishmentGroup.MultiAcademyTrustGroupTypeCode);
        audiences[SatAudience] = new("SAT", "SAT", fileName, ["GroupUID", "LA", "Estab", .. pupilHeader], EstablishmentGroup.SingleAcademyTrustGroupTypeCode);
        audiences[LaAlternativeProvisionAudience] = new("LA alternative provision", "LA", $"pp{year}_*_AP_{maturity}", ["LA", "AlternativeProvision", .. pupilHeader]);

        return audiences;
    }

    private static async IAsyncEnumerable<PreparedDownloadRow> ReadRowsAsync(
        DataFileReader reader,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        int la = reader.GetColumn(LaColumn);
        int estab = reader.GetColumn(EstabColumn);
        int urn = reader.GetColumn(UrnColumn);
        int laEstab = reader.GetColumn(LaEstabColumn);
        int source = reader.GetColumn(SourceColumn);
        int modService = reader.GetColumn(ModServiceColumn);
        int censusServiceEver6 = reader.GetColumn(CensusServiceEver6Column);
        int?[] pupilColumns = [.. _pupilColumns.Select(c => c.Column is null ? (int?)null : reader.GetColumn(c.Column))];
        HashSet<int> dateColumns = [.. _dateColumns.Select(reader.GetColumn)];

        await foreach (DataFileRow row in reader.ReadRowsAsync(cancellationToken))
        {
            string?[] values = row.Values;

            if (values[source] == CensusSource)
            {
                // Only school files include pupils whose service child premium comes from the MoD
                string? serviceChildPremium = values[censusServiceEver6];
                string? schoolServiceChildPremium = values[modService] == "1" && serviceChildPremium == "0" ? "1" : serviceChildPremium;

                yield return new PreparedDownloadRow(LaAudience, values[la], values[la], row.RowNumber, [values[la], values[estab], .. GetPupilValues(serviceChildPremium)]);
                yield return new PreparedDownloadRow(SchoolAudience, values[urn], values[laEstab], row.RowNumber, GetPupilValues(schoolServiceChildPremium));
                yield return new PreparedDownloadRow(MatAudience, values[urn], null, row.RowNumber, [values[la], values[estab], .. GetPupilValues(serviceChildPremium)]);
                yield return new PreparedDownloadRow(SatAudience, values[urn], null, row.RowNumber, [values[la], values[estab], .. GetPupilValues(serviceChildPremium)]);
            }
            else if (values[source] == AlternativeProvisionSource)
            {
                yield return new PreparedDownloadRow(
                    LaAlternativeProvisionAudience,
                    values[la],
                    values[la],
                    row.RowNumber,
                    [values[la], "ALT", .. GetPupilValues(values[censusServiceEver6])]);
            }

            string?[] GetPupilValues(string? serviceChildPremium) =>
                [.. pupilColumns.Select(c => c is not { } column ? serviceChildPremium :
                    dateColumns.Contains(column) ? FormatDate(values[column]) :
                    values[column])];
        }
    }

    // As yyyy-MM-dd; a value that isn't a valid date is empty
    private static string? FormatDate(string? value) =>
        value is not null && DataFileReader.ParseDate(value) is { } date
            ? date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            : null;
}
