namespace DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;

/// <summary>
/// A zip uploaded for ingestion, identified by where it was uploaded.
/// </summary>
public class IngestFile
{
    /// <summary>
    /// The dataset folder and zip name, e.g. <c>ks2/NPD_KS2_2025_U.zip</c>.
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// Where the zip is now: in the folder's <c>upload/</c>, <c>processed/</c> or <c>error/</c>.
    /// </summary>
    public required string BlobName { get; set; }

    public required IngestFileStatus Status { get; set; }

    /// <summary>
    /// Why the zip as a whole failed, e.g. its name doesn't match its folder. Failures of the data files in it are on
    /// their <see cref="IngestRun"/>s.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// The number of times the zip has been processed; incremented when processing starts.
    /// </summary>
    public int Attempts { get; set; }

    public required DateTime FirstSeenAt { get; init; }
    public DateTime? LastProcessedAt { get; set; }
}

public enum IngestFileStatus
{
    /// <summary>
    /// Waiting to be processed.
    /// </summary>
    Pending,
    Processing,
    Succeeded,
    Failed
}
