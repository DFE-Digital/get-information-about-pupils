using DfE.GIAP.Core.Common.Domain;

namespace DfE.GIAP.Core.User.Domain.Aggregate;
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
