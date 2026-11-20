using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using DfE.GIAP.Core.Ingest;

namespace DfE.GIAP.Core.PupilData;

/// <summary>
/// Writes further education data files: FE pupil premium and FE SEN.
/// </summary>
/// <remarks>
/// FE files have no maturity rule, as in the old pipeline; a file's maturity is ignored.
/// </remarks>
public sealed class FurtherEducationDatasetProcessor(FurtherEducationDataWriter writer) : IIngestDatasetProcessor
{
    private static readonly Dictionary<IngestDataset, FurtherEducationDataset> _datasets = new()
    {
        [IngestDataset.FurtherEducationPupilPremium] = FurtherEducationDataset.PupilPremium,
        [IngestDataset.FurtherEducationSpecialEducationalNeeds] = FurtherEducationDataset.SpecialEducationalNeeds
    };

    public IReadOnlyCollection<IngestDataset> Datasets { get; } = [.. _datasets.Keys];

    public async Task<IngestDataFileResult> ProcessAsync(IngestDataFile file, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(file);

        using FurtherEducationDataFileReader reader = await FurtherEducationDataFileReader.OpenAsync(
            file.Content,
            file.Attributes.AcademicYear,
            cancellationToken);

        // Find the first valid row before writing, so a file with none fails with the reason
        await using IAsyncEnumerator<FurtherEducationFileRow> rows = reader.ReadRowsAsync(cancellationToken).GetAsyncEnumerator(cancellationToken);

        if (!await rows.MoveNextAsync())
        {
            throw new IngestDataFileException(reader.RejectedRows.Count == 0
                ? "The data file has no rows."
                : $"All {reader.RejectedRows.Count} rows in the data file were rejected. Row {reader.RejectedRows[0].RowNumber}: " +
                  string.Join(" ", reader.RejectedRows[0].Reasons));
        }

        FurtherEducationFileWriteResult result = await writer.WriteFileAsync(
            _datasets[file.Attributes.Dataset],
            file.Attributes.AcademicYear,
            ReadRemainingAsync(rows),
            cancellationToken);

        return new IngestDataFileResult(
            RecordsRead: reader.RowsRead,
            RecordsWritten: result.RecordsWritten,
            RecordsRemoved: result.RecordsRemoved,
            RejectedRows: reader.RejectedRows);
    }

    private static async IAsyncEnumerable<FurtherEducationFileRow> ReadRemainingAsync(IAsyncEnumerator<FurtherEducationFileRow> rows)
    {
        do
        {
            yield return rows.Current;
        }
        while (await rows.MoveNextAsync());
    }
}
