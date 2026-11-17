namespace DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;

/// <remarks>
/// Every establishment in the GIAS extract is stored, including closed, Welsh and overseas establishments;
/// consumers apply any filters they need.
/// </remarks>
public class Establishment
{
    public required int Urn { get; init; }
    public required int LaCode { get; init; }
    public required int? EstablishmentNumber { get; init; }
    public required string Name { get; init; }
    public required long? Uprn { get; init; }
    public required int TypeOfEstablishmentCode { get; init; }
    public required string TypeOfEstablishmentName { get; init; }
    public required int EstablishmentStatusCode { get; init; }
    public required string EstablishmentStatusName { get; init; }
    public required DateOnly? CloseDate { get; init; }
    public required int PhaseOfEducationCode { get; init; }
    public required string PhaseOfEducationName { get; init; }
    public required int? StatutoryLowAge { get; init; }
    public required int? StatutoryHighAge { get; init; }

    /// <summary>
    /// The government office region code; <c>W</c> is Wales and <c>Z</c> is overseas or offshore.
    /// </summary>
    public required string? GorCode { get; init; }
}
