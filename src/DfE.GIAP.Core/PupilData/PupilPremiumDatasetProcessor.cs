using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using DfE.GIAP.Core.Ingest;

namespace DfE.GIAP.Core.PupilData;

/// <summary>
/// Writes pupil premium data files.
/// </summary>
public sealed class PupilPremiumDatasetProcessor(PupilPremiumDataWriter writer) : IIngestDatasetProcessor
{
    public IReadOnlyCollection<IngestDataset> Datasets { get; } = [IngestDataset.PupilPremium];

    public async Task<IngestDataFileResult> ProcessAsync(IngestDataFile file, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(file);

        // As in the old pipeline, which named the prepared downloads after the U or F
        if (file.Attributes.IngestVersion is not (IngestVersion.Unamended or IngestVersion.Final))
        {
            throw new IngestDataFileException($"A pupil premium file must be U or F, not {file.Attributes.IngestVersion}.");
        }

        using PupilPremiumDataFileReader reader = await PupilPremiumDataFileReader.OpenAsync(file.Content, cancellationToken);

        // Find the first valid row before writing, so a file with none fails with the reason
        await using IAsyncEnumerator<PupilPremiumFileRow> rows = reader.ReadRowsAsync(cancellationToken).GetAsyncEnumerator(cancellationToken);

        if (!await rows.MoveNextAsync())
        {
            throw new IngestDataFileException(reader.RejectedRows.Count == 0
                ? "The data file has no rows."
                : $"All {reader.RejectedRows.Count} rows in the data file were rejected. Row {reader.RejectedRows[0].RowNumber}: " +
                  string.Join(" ", reader.RejectedRows[0].Reasons));
        }

        PupilPremiumFileWriteResult result = await writer.WriteFileAsync(
            file.Attributes.AcademicYear,
            file.Attributes.IngestVersion,
            ReadRemainingAsync(rows),
            cancellationToken);

        return new IngestDataFileResult(
            RecordsRead: reader.RowsRead,
            RecordsWritten: result.RecordsWritten,
            RecordsRemoved: result.RecordsRemoved,
            RejectedRows: reader.RejectedRows);
    }

    private static async IAsyncEnumerable<PupilPremiumFileRow> ReadRemainingAsync(IAsyncEnumerator<PupilPremiumFileRow> rows)
    {
        do
        {
            yield return rows.Current;
        }
        while (await rows.MoveNextAsync());
    }
}
