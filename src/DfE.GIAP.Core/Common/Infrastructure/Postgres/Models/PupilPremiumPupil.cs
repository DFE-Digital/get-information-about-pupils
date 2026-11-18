namespace DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;

/// <remarks>
/// Only the latest pupil premium file is kept; each file replaces all the data.
/// </remarks>
public class PupilPremiumPupil
{
    public required string PupilMatchingRef { get; init; }

    /// <summary>
    /// Not unique; a UPN can appear under more than one <see cref="PupilMatchingRef"/>.
    /// </summary>
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
    /// The pupil's <c>Source</c> from the file, or <c>Both</c> if their rows have different sources.
    /// </summary>
    public required string? SourceFlag { get; init; }

    /// <summary>
    /// A JSON array of the pupil's rows in the file.
    /// </summary>
    public required string Data { get; init; }
}
