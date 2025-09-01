using System.Text.Json;
using DfE.GIAP.Web.Features.Session.Infrastructure.KeyResolver;
using DfE.GIAP.Web.Features.Session.Infrastructure.Provider;

namespace DfE.GIAP.Web.Features.Session.Query;

public sealed class AspNetCoreSessionQueryHandler<TSessionObject> : ISessionQueryHandler<TSessionObject>
{
    private readonly IAspNetCoreSessionProvider _sessionProvider;
    private readonly ISessionObjectKeyResolver _sessionKeyResolver;

    public AspNetCoreSessionQueryHandler(
        IAspNetCoreSessionProvider sessionProvider,
        ISessionObjectKeyResolver sessionKeyResolver)
    {
        ArgumentNullException.ThrowIfNull(sessionProvider);
        _sessionProvider = sessionProvider;

        ArgumentNullException.ThrowIfNull(sessionKeyResolver);
        _sessionKeyResolver = sessionKeyResolver;
    }

    public SessionQueryResponse<TSessionObject> Get()
    {
        string sessionObjectKey = _sessionKeyResolver.Resolve(typeof(TSessionObject));

        ISession session = _sessionProvider.GetSession();

        if (!session.ContainsKey(sessionObjectKey))
        {
            return new SessionQueryResponse<TSessionObject>(
                result: default,
                valueExists: false);
        }

        string sessionValue = session.GetString(sessionObjectKey)!;

        TSessionObject outputValue = JsonSerializer.Deserialize<TSessionObject>(sessionValue)!;

        return new SessionQueryResponse<TSessionObject>(
            result: outputValue,
            valueExists: true);
    }
}
