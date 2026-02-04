using System.Text;
using DfE.GIAP.Web.Shared.Session.Abstraction;
using DfE.GIAP.Web.Shared.Session.Abstraction.Query;

namespace DfE.GIAP.Web.Shared.Session.Infrastructure.Query;

public sealed class AspNetCoreSessionQueryHandler<TSessionObject> : ISessionQueryHandler<TSessionObject> where TSessionObject : class
{
    private readonly IAspNetCoreSessionProvider _sessionProvider;
    private readonly ISessionObjectSerializer<TSessionObject> _sessionObjectSerializer;

    public AspNetCoreSessionQueryHandler(
        IAspNetCoreSessionProvider sessionProvider,
        ISessionObjectSerializer<TSessionObject> sessionObjectSerializer)
    {
        ArgumentNullException.ThrowIfNull(sessionProvider);
        _sessionProvider = sessionProvider;

        ArgumentNullException.ThrowIfNull(sessionObjectSerializer);
        _sessionObjectSerializer = sessionObjectSerializer;
    }

    public SessionQueryResponse<TSessionObject> Handle(SessionCacheKey key)
    {
        ArgumentNullException.ThrowIfNull(key);

        ISession session = _sessionProvider.GetSession();

        if (!session.TryGetValue(key.Value, out byte[] value))
        {
            return SessionQueryResponse<TSessionObject>.CreateWithNoValue();
        }

        TSessionObject outputValue =
            _sessionObjectSerializer.Deserialize(
                input: Encoding.UTF8.GetString(value))!;

        return SessionQueryResponse<TSessionObject>.Create(outputValue);
    }
}
