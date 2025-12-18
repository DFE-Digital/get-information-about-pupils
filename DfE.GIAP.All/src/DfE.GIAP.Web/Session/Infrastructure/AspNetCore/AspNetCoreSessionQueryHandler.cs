using DfE.GIAP.Web.Session.Abstraction.Query;
using DfE.GIAP.Web.Session.Abstraction;

namespace DfE.GIAP.Web.Session.Infrastructure.AspNetCore;

public sealed class AspNetCoreSessionQueryHandler<TSessionObject> : ISessionQueryHandler<TSessionObject> where TSessionObject : class
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

    public SessionQueryResponse<TSessionObject> Handle()
    {
        ISession session = _sessionProvider.GetSession();

        string sessionObjectAccessKey = _sessionKeyResolver.Resolve<TSessionObject>();

        if (string.IsNullOrWhiteSpace(sessionObjectAccessKey) || !session.TryGetValue(sessionObjectAccessKey, out byte[] _))
        {
            return SessionQueryResponse<TSessionObject>.CreateWithNoValue();
        }

        string sessionValue = session.GetString(sessionObjectAccessKey);

        TSessionObject outputValue = _sessionObjectSerializer.Deserialize(sessionValue)!;

        return SessionQueryResponse<TSessionObject>.Create(outputValue);
    }
}
