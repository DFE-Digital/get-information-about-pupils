using DfE.GIAP.Core.Common.Domain;

namespace DfE.GIAP.Core.User.Application;
public sealed class UserId : ValueObject<UserId>
{
    public UserId(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        Value = id;
    }
    public string Value { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
