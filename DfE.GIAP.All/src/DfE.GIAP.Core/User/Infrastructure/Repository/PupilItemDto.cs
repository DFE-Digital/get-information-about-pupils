namespace DfE.GIAP.Core.User.Infrastructure.Repository;
public sealed class PupilItemDto
{
    public string? PupilId { get; set; }
    // public bool IsMasked { get; set; } // TODO Do we still need this given Masked is evaluated by the Domain.
}
