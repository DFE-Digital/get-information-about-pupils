namespace DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;

/// <remarks>
/// Derived from the pupil's <see cref="NpdPupilRecord"/>s; the identifying fields are taken from a census record
/// if there is one, then from the latest academic year, then from the record that was loaded first.
/// </remarks>
public class NpdPupil
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
}
