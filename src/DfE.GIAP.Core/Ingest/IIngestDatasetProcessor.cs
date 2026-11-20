using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;

namespace DfE.GIAP.Core.Ingest;

/// <summary>
/// Writes the data files for one or more datasets.
/// </summary>
/// <remarks>
/// A processor commits each data file on its own, and must be safe to run again on the same file so that it can be
/// replayed. Throwing fails the data file; rows that can't be written but don't fail the file should be returned in
/// <see cref="IngestDataFileResult.RejectedRows"/>.
/// </remarks>
public interface IIngestDatasetProcessor
{
    IReadOnlyCollection<IngestDataset> Datasets { get; }

    Task<IngestDataFileResult> ProcessAsync(IngestDataFile file, CancellationToken cancellationToken);
}

/// <param name="Name">The data file's name inside the zip.</param>
/// <param name="Content">The data file's uncompressed content; it can only be read once.</param>
public sealed record IngestDataFile(string Name, IngestDataFileAttributes Attributes, Stream Content);

public sealed record IngestDataFileResult(
    int RecordsRead,
    int RecordsWritten,
    int RecordsRemoved,
    IReadOnlyCollection<IngestRowRejection> RejectedRows);

/// <param name="RowNumber">The row's 1-based position in the data file, not counting the header.</param>
/// <param name="Row">The row's values by column name.</param>
public sealed record IngestRowRejection(int RowNumber, IReadOnlyDictionary<string, string?> Row, IReadOnlyList<string> Reasons);
