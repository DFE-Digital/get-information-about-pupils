namespace DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;

/// <summary>
/// A learner's data for one academic year from a further education file. The identifying fields are as they appear in
/// that file.
/// </summary>
/// <remarks>
/// FE files have no ingest version; a later file for the same dataset and year replaces the earlier one.
/// A pupil premium file replaces all FE data, including special educational needs.
/// </remarks>
public class FurtherEducationLearnerRecord
{
    public required string Uln { get; init; }
    public required FurtherEducationDataset Dataset { get; init; }

    /// <summary>
    /// As it appears in the file, e.g. <c>2024/2025</c>.
    /// </summary>
    public required string AcademicYear { get; init; }

    public required string? Surname { get; init; }
    public required string? Forename { get; init; }
    public required string? Sex { get; init; }
    public required DateOnly? Dob { get; init; }

    /// <summary>
    /// A JSON array of the learner's rows in the file for this academic year.
    /// </summary>
    public required string Data { get; init; }

    /// <summary>
    /// When the record was first written. It's set by the database.
    /// </summary>
    public DateTime CreatedAt { get; init; }
}
