namespace DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;

/// <remarks>
/// Derived from the learner's <see cref="FurtherEducationLearnerRecord"/>s; the identifying fields are taken from a
/// pupil premium record if there is one, then from the latest academic year, then from the record that was loaded first.
/// </remarks>
public class FurtherEducationLearner
{
    public required string Uln { get; init; }
    public required string? Surname { get; init; }
    public required string? Forename { get; init; }
    public required string? Sex { get; init; }
    public required DateOnly? Dob { get; init; }
}
