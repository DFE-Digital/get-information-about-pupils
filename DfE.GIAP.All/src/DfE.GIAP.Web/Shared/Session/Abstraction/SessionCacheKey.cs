using DfE.CleanArchitecture.Common.Domain;

namespace DfE.GIAP.Web.Shared.Session.Abstraction;

public sealed class SessionCacheKey : ValueObject<SessionCacheKey>
{
    public SessionCacheKey(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }
    public string Value { get; }
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
