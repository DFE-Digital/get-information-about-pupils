namespace DfE.GIAP.Core.User.Infrastructure.Repository;
public sealed class PupilItemDto
{
    public Guid Id { get; set; }
    public string PupilId { get; set; } = string.Empty; // UPN
    // public bool IsMasked { get; set; } // TODO Do we still need this given Masked is evaluated by the Domain.
}
