using DfE.GIAP.Core.Common.Domain;

namespace DfE.GIAP.Core.MyPupils.Domain;
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
