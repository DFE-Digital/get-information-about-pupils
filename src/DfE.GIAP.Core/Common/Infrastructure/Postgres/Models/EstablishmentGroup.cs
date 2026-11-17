namespace DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;

public class EstablishmentGroup
{
    public const int MultiAcademyTrustGroupTypeCode = 6;
    public const int SingleAcademyTrustGroupTypeCode = 10;
    public const string OpenGroupStatusCode = "OPEN";

    public required int GroupUid { get; init; }
    public required string? GroupId { get; init; }
    public required string Name { get; init; }
    public required int GroupTypeCode { get; init; }
    public required string GroupTypeName { get; init; }
    public required string GroupStatusCode { get; init; }
    public required string GroupStatusName { get; init; }
    public required DateOnly? ClosedDate { get; init; }
}
