using DfE.GIAP.Web.Features.Session.Infrastructure.KeyResolver;
using DfE.GIAP.Web.Features.Session.Infrastructure.Provider;
using DfE.GIAP.Web.Features.Session.Infrastructure.Serialization;

namespace DfE.GIAP.Web.Features.Session.Query;

public sealed class AspNetCoreSessionQueryHandler<TSessionObject> : ISessionQueryHandler<TSessionObject>
{
    private readonly IAspNetCoreSessionProvider _sessionProvider;
    private readonly ISessionObjectKeyResolver _sessionKeyResolver;
    private readonly ISessionObjectSerializer<TSessionObject> _sessionObjectSerializer;

    public AspNetCoreSessionQueryHandler(
        IAspNetCoreSessionProvider sessionProvider,
        ISessionObjectKeyResolver sessionKeyResolver,
        ISessionObjectSerializer<TSessionObject> sessionObjectSerializer)
    {
        ArgumentNullException.ThrowIfNull(sessionProvider);
        _sessionProvider = sessionProvider;

        ArgumentNullException.ThrowIfNull(sessionKeyResolver);
        _sessionKeyResolver = sessionKeyResolver;

        ArgumentNullException.ThrowIfNull(sessionObjectSerializer);
        _sessionObjectSerializer = sessionObjectSerializer;
    }

    public SessionQueryResponse<TSessionObject> GetSessionObject()
    {
        string sessionObjectKey = _sessionKeyResolver.Resolve<TSessionObject>();

        ISession session = _sessionProvider.GetSession();

        if ((string.IsNullOrWhiteSpace(sessionObjectKey)) || !session.TryGetValue(sessionObjectKey, out byte[] _))
        {
            return new SessionQueryResponse<TSessionObject>(
                result: default,
                valueExists: false);
        }

        string sessionValue = session.GetString(sessionObjectKey);

        TSessionObject outputValue = _sessionObjectSerializer.Deserialize(input: sessionValue)!;

        return new SessionQueryResponse<TSessionObject>(
            result: outputValue,
            valueExists: true);
    }

}
