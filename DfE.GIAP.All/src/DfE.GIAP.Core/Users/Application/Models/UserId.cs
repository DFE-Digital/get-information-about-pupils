using DfE.GIAP.Core.Common.Domain;

namespace DfE.GIAP.Core.Users.Application.Models;
public sealed class UserId : ValueObject<UserId>
{
    public string Value { get; }
    public UserId(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        Value = id;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
