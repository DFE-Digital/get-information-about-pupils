using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using DfE.GIAP.Core.Ingest;

namespace DfE.GIAP.Core.PupilData;

/// <summary>
/// Writes NPD data files: census, EYFSP, phonics, KS1, KS2, KS4 and MTC.
/// </summary>
public sealed class NpdDatasetProcessor(NpdDataWriter writer) : IIngestDatasetProcessor
{
    public IReadOnlyCollection<IngestDataset> Datasets { get; } = [.. NpdDatasetDefinition.All.Keys];

    public async Task<IngestDataFileResult> ProcessAsync(IngestDataFile file, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(file);

        NpdDatasetDefinition definition = NpdDatasetDefinition.All[file.Attributes.Dataset];

        using NpdDataFileReader reader = await NpdDataFileReader.OpenAsync(
            file.Content,
            definition,
            file.Attributes.AcademicYear,
            cancellationToken);

        // Find the first valid row before writing, so a file with none fails with the reason
        await using IAsyncEnumerator<NpdFileRow> rows = reader.ReadRowsAsync(cancellationToken).GetAsyncEnumerator(cancellationToken);

        if (!await rows.MoveNextAsync())
        {
            throw new IngestDataFileException(reader.RejectedRows.Count == 0
                ? "The data file has no rows."
                : $"All {reader.RejectedRows.Count} rows in the data file were rejected. Row {reader.RejectedRows[0].RowNumber}: " +
                  string.Join(" ", reader.RejectedRows[0].Reasons));
        }

        NpdFileWriteResult result = await writer.WriteFileAsync(
            definition.Dataset,
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

    private static async IAsyncEnumerable<NpdFileRow> ReadRemainingAsync(IAsyncEnumerator<NpdFileRow> rows)
    {
        do
        {
            yield return rows.Current;
        }
        while (await rows.MoveNextAsync());
    }
}
