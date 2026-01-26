using DfE.GIAP.Web.Shared.Serializer;
using DfE.GIAP.Web.Shared.Session.Abstraction;

namespace DfE.GIAP.Web.Shared.Session.Infrastructure.Serialization;

public class DefaultSessionObjectSerializer<TSessionObject> : ISessionObjectSerializer<TSessionObject> where TSessionObject : class
{
    private readonly IJsonSerializer _jsonSerializer;
    public DefaultSessionObjectSerializer(IJsonSerializer jsonSerializer)
    {
        ArgumentNullException.ThrowIfNull(jsonSerializer);
        _jsonSerializer = jsonSerializer;
    }
    public TSessionObject Deserialize(string input) => _jsonSerializer.Deserialize<TSessionObject>(input);
    public string Serialize(TSessionObject sessionObject) => _jsonSerializer.Serialize(sessionObject);
}
