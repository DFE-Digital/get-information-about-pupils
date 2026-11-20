using System.Runtime.CompilerServices;
using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using DfE.GIAP.Core.Ingest;

namespace DfE.GIAP.Core.PupilData;

/// <summary>
/// Writes the prepared downloads from KS2 and KS4 children looked after (CLA) data files: a file for each LA. Nothing
/// is written to Postgres.
/// </summary>
/// <remarks>
/// Replaces the old pipeline's <c>giapKS2CLASplit</c> and <c>giapKS4CLASplit</c> pipelines and
/// <c>create_partitions_cla</c> notebook. Every column in the data file is written, in the order it's in the file.
/// </remarks>
public sealed class ChildrenLookedAfterDatasetProcessor(PreparedDownloadWriter writer) : IIngestDatasetProcessor
{
    private const string LaColumn = "CLA_LA";

    // The old pipeline's ks4_cla_header_fields, which it used for KS2 files too
    private static readonly string[] _requiredColumns =
        [LaColumn, "CLA_CHILD_LA_CODE", "CLA_SEX", "CLA_DOB", "CLA_AGE_AUG", "CLA_UPN", "CLA_ETHNIC", "CLA_12_MONTHS"];

    public IReadOnlyCollection<IngestDataset> Datasets { get; } =
        [IngestDataset.ChildrenLookedAfterKs2, IngestDataset.ChildrenLookedAfterKs4];

    public async Task<IngestDataFileResult> ProcessAsync(IngestDataFile file, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(file);

        using DataFileReader reader = await DataFileReader.OpenAsync(file.Content, _requiredColumns, cancellationToken);
        int la = reader.GetColumn(LaColumn);

        // The old pipeline's file names, e.g. ks4_2025_cla for 2024/2025
        string keyStage = file.Attributes.Dataset == IngestDataset.ChildrenLookedAfterKs2 ? "ks2" : "ks4";
        PreparedDownloadAudience[] audiences =
            [new("LA", "LA", $"{keyStage}_{file.Attributes.AcademicYear[5..]}_cla", [.. reader.Header])];

        int rowsWithoutLa = 0;

        await writer.WriteAsync(audiences, ReadRowsAsync(cancellationToken), cancellationToken);

        if (reader.RowsRead == 0)
        {
            throw new IngestDataFileException("The data file has no rows.");
        }

        return new IngestDataFileResult(
            RecordsRead: reader.RowsRead,
            RecordsWritten: reader.RowsRead - rowsWithoutLa,
            RecordsRemoved: 0,
            RejectedRows: []);

        async IAsyncEnumerable<PreparedDownloadRow> ReadRowsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (DataFileRow row in reader.ReadRowsAsync(cancellationToken))
            {
                if (row.Values[la] is null)
                {
                    rowsWithoutLa++;
                }

                yield return new PreparedDownloadRow(0, row.Values[la], row.Values[la], row.RowNumber, row.Values);
            }
        }
    }
}
