using DfE.GIAP.Web.Features.Session.Infrastructure.KeyResolver;
using DfE.GIAP.Web.Features.Session.Infrastructure.Provider;
using DfE.GIAP.Web.Features.Session.Infrastructure.Serialization;

namespace DfE.GIAP.Web.Features.Session.Command;

public sealed class AspNetCoreSessionCommandHandler<TValue> : ISessionCommandHandler<TValue>
{
    private readonly IAspNetCoreSessionProvider _sessionProvider;
    private readonly ISessionObjectKeyResolver _sessionKeyResolver;
    private readonly ISessionObjectSerializer<TValue> _sessionObjectSerializer;

    public AspNetCoreSessionCommandHandler(
        IAspNetCoreSessionProvider sessionProvider,
        ISessionObjectKeyResolver sessionKeyResolver,
        ISessionObjectSerializer<TValue> sessionObjectSerializer)
    {
        ArgumentNullException.ThrowIfNull(sessionProvider);
        _sessionProvider = sessionProvider;

        ArgumentNullException.ThrowIfNull(sessionKeyResolver);
        _sessionKeyResolver = sessionKeyResolver;

        ArgumentNullException.ThrowIfNull(sessionObjectSerializer);
        _sessionObjectSerializer = sessionObjectSerializer;
    }

    private string SessionObjectAccessKey => _sessionKeyResolver.Resolve(typeof(TValue));

    public void StoreInSession(TValue value)
    {
        ArgumentNullException.ThrowIfNull(value);

        string json = _sessionObjectSerializer.Serialize(value);

        _sessionProvider
            .GetSession()
            .SetString(SessionObjectAccessKey, json);
    }
}
