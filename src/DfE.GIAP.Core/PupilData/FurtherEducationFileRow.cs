namespace DfE.GIAP.Core.PupilData;

/// <summary>
/// A row from a further education file. A learner's rows are combined into a single <see cref="Common.Infrastructure.Postgres.Models.FurtherEducationLearnerRecord"/>.
/// </summary>
/// <param name="RowNumber">The row's position in the file; a learner's rows are combined in this order, and the
/// identifying fields are taken from the first.</param>
/// <param name="Data">The row's values as a JSON object.</param>
public sealed record FurtherEducationFileRow(
    int RowNumber,
    string Uln,
    string? Surname,
    string? Forename,
    string? Sex,
    DateOnly? Dob,
    string Data);
