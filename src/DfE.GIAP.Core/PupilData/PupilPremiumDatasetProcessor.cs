using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using DfE.GIAP.Core.Ingest;

namespace DfE.GIAP.Core.PupilData;

/// <summary>
/// Writes pupil premium data files, then their prepared downloads.
/// </summary>
/// <remarks>
/// The data file is copied to a temp file, since it's read more than once. The prepared downloads are only written once
/// the pupils have been, so a file that fails (e.g. one older than the stored data) writes none.
/// </remarks>
public sealed class PupilPremiumDatasetProcessor(PupilPremiumDataWriter writer, PupilPremiumPreparedDownloads preparedDownloads)
    : IIngestDatasetProcessor
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

        string tempPath = Path.GetTempFileName();

        try
        {
            await using (FileStream copy = new(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 81920, useAsync: true))
            {
                await file.Content.CopyToAsync(copy, cancellationToken);
            }

            IngestDataFileResult result = await WritePupilsAsync(file, tempPath, cancellationToken);

            await using (FileStream content = OpenRead(tempPath))
            {
                await preparedDownloads.WriteAsync(content, file.Attributes, cancellationToken);
            }

            return result;
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    private async Task<IngestDataFileResult> WritePupilsAsync(IngestDataFile file, string path, CancellationToken cancellationToken)
    {
        using PupilPremiumDataFileReader reader = await PupilPremiumDataFileReader.OpenAsync(OpenRead(path), cancellationToken);

        // Before anything is written, so a file the prepared downloads can't be written from doesn't replace the pupils
        await using (FileStream content = OpenRead(path))
        {
            await PupilPremiumPreparedDownloads.ValidateColumnsAsync(content, cancellationToken);
        }

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

    private static FileStream OpenRead(string path) =>
        new(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 81920, FileOptions.Asynchronous | FileOptions.SequentialScan);

    private static async IAsyncEnumerable<PupilPremiumFileRow> ReadRemainingAsync(IAsyncEnumerator<PupilPremiumFileRow> rows)
    {
        do
        {
            yield return rows.Current;
        }
        while (await rows.MoveNextAsync());
    }
}
