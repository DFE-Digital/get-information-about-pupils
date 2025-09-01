using DfE.GIAP.Web.Session.Abstraction.Query;
using DfE.GIAP.Web.Session.Abstraction;

namespace DfE.GIAP.Web.Session.Infrastructure.AspNetCore.Query;

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

    private string SessionObjectKey => _sessionKeyResolver.Resolve<TSessionObject>();

    public SessionQueryResponse<TSessionObject> GetSessionObject()
    {
        ISession session = _sessionProvider.GetSession();

        if (string.IsNullOrWhiteSpace(SessionObjectKey) || !session.TryGetValue(SessionObjectKey, out byte[] _))
        {
            return SessionQueryResponse<TSessionObject>.NoValue();
        }

        string sessionValue = session.GetString(SessionObjectKey);

        TSessionObject outputValue = _sessionObjectSerializer.Deserialize(sessionValue)!;

        return SessionQueryResponse<TSessionObject>.Create(outputValue);
    }
}
