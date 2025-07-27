namespace DfE.GIAP.Core.User.Infrastructure.Repository;
public sealed class MyPupilItemDto
{
    public Guid Id { get; set; }
    public string UPN { get; set; } = string.Empty;
    // public bool IsMasked { get; set; } // TODO Do we still need this given Masked is evaluated by the Domain.
}
