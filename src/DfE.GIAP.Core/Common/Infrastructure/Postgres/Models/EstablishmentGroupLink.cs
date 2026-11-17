namespace DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;

/// <remarks>
/// There's no foreign key from <see cref="Urn"/> to <see cref="Establishment"/>; the links extract includes
/// establishments (e.g. children's centres) that aren't in the all establishment data extract.
/// </remarks>
public class EstablishmentGroupLink
{
    public required int GroupUid { get; init; }
    public required int Urn { get; init; }
    public required DateOnly? JoinedDate { get; init; }
}
