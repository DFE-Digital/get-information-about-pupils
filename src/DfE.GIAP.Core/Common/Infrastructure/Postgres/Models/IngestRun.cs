namespace DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;

/// <summary>
/// One attempt at processing a data file from an <see cref="IngestFile"/>.
/// </summary>
public class IngestRun
{
    public required Guid Id { get; init; }
    public required string FilePath { get; init; }

    /// <summary>
    /// The <see cref="IngestFile.Attempts"/> this run was part of.
    /// </summary>
    public required int Attempt { get; init; }

    /// <summary>
    /// The data file's name inside the zip.
    /// </summary>
    public required string DataFileName { get; init; }

    /// <summary>
    /// The attributes parsed from <see cref="DataFileName"/>; <see langword="null"/> if it couldn't be parsed.
    /// </summary>
    public IngestDataset? Dataset { get; set; }

    /// <inheritdoc cref="NpdPupilRecord.AcademicYear"/>
    public string? AcademicYear { get; set; }

    public IngestVersion? IngestVersion { get; set; }

    public int? RecordsRead { get; set; }
    public int? RecordsWritten { get; set; }
    public int? RecordsRemoved { get; set; }
    public int? RecordsRejected { get; set; }

    public required IngestRunStatus Status { get; set; }
    public string? Error { get; set; }
    public required DateTime StartedAt { get; init; }
    public DateTime? FinishedAt { get; set; }
}

public enum IngestRunStatus
{
    Running,
    Succeeded,
    Failed
}
