using DfE.GIAP.Web.Shared.Session.Abstraction;
using DfE.GIAP.Web.Shared.Session.Abstraction.Command;

namespace DfE.GIAP.Web.Shared.Session.Infrastructure.Command;

public sealed class AspNetCoreSessionCommandHandler<TValue> : ISessionCommandHandler<TValue> where TValue : class
{
    private readonly IAspNetCoreSessionProvider _sessionProvider;
    private readonly ISessionObjectSerializer<TValue> _sessionObjectSerializer;

    public AspNetCoreSessionCommandHandler(
        IAspNetCoreSessionProvider sessionProvider,
        ISessionObjectSerializer<TValue> sessionObjectSerializer)
    {
        ArgumentNullException.ThrowIfNull(sessionProvider);
        _sessionProvider = sessionProvider;

        ArgumentNullException.ThrowIfNull(sessionObjectSerializer);
        _sessionObjectSerializer = sessionObjectSerializer;
    }

    public void StoreInSession(SessionCacheKey key, TValue value)
    {
        ArgumentNullException.ThrowIfNull(value);

        string json = _sessionObjectSerializer.Serialize(value);

        _sessionProvider
            .GetSession()
            .SetString(
                key: key.Value,
                value: json);
    }
}
