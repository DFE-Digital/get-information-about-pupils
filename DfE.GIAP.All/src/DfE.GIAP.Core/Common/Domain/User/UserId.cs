using DfE.GIAP.Core.Common.Domain.Contracts;

namespace DfE.GIAP.Core.Common.Domain.User;
public sealed class UserId : ValueObject<UserId>
{
    public UserId(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        Id = id;
    }
    public string Id { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
