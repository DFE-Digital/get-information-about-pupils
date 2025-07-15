using DfE.GIAP.Core.Common.Domain.Contracts;

namespace DfE.GIAP.Core.Common.Domain.User;
public sealed class UserIdentifier : ValueObject<UserIdentifier>
{
    public UserIdentifier(string id)
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
