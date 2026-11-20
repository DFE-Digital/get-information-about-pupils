namespace DfE.GIAP.Core.PupilData;

/// <summary>
/// A row from a pupil premium file. A pupil's rows are combined into a single <see cref="Common.Infrastructure.Postgres.Models.PupilPremiumPupil"/>.
/// </summary>
/// <param name="RowNumber">The row's position in the file; a pupil's rows are combined in this order, and the
/// identifying fields are taken from the first.</param>
/// <param name="Data">The row's values as a JSON object.</param>
public sealed record PupilPremiumFileRow(
    int RowNumber,
    string PupilMatchingRef,
    string? Upn,
    int? LaCode,
    int? EstablishmentNumber,
    int? Urn,
    string? Surname,
    string? Forename,
    string? MiddleNames,
    string? Sex,
    DateOnly? Dob,
    string? Source,
    string Data);
