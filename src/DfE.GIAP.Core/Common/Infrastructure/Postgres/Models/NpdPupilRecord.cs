namespace DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;

/// <summary>
/// A pupil's data from a single NPD file. The identifying fields are as they appear in that file.
/// </summary>
public class NpdPupilRecord
{
    public required string PupilMatchingRef { get; init; }
    public required NpdDataset Dataset { get; init; }

    /// <summary>
    /// As it appears in the file, e.g. <c>2024/2025</c>.
    /// </summary>
    public required string AcademicYear { get; init; }

    public required IngestVersion IngestVersion { get; init; }
    public required string? Upn { get; init; }
    public required int? LaCode { get; init; }
    public required int? EstablishmentNumber { get; init; }
    public required int? Urn { get; init; }
    public required string? Surname { get; init; }
    public required string? Forename { get; init; }
    public required string? MiddleNames { get; init; }
    public required string? Sex { get; init; }
    public required DateOnly? Dob { get; init; }

    /// <summary>
    /// A JSON array of the pupil's rows in the file; a pupil can have more than one
    /// (e.g. a dual-registered pupil appears in the census once for each school).
    /// </summary>
    public required string Data { get; init; }

    /// <summary>
    /// When this version of the record was first written. It's set by the database.
    /// </summary>
    public DateTime CreatedAt { get; init; }
}
