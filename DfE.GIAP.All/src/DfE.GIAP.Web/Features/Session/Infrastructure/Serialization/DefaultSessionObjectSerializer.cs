using DfE.GIAP.Web.Features.Session.Abstractions;
using Newtonsoft.Json;

namespace DfE.GIAP.Web.Features.Session.Infrastructure.Serialization;

public class DefaultSessionObjectSerializer<TSessionObject> : ISessionObjectSerializer<TSessionObject>
{
    public TSessionObject Deserialize(string input) => JsonConvert.DeserializeObject<TSessionObject>(input);
    public string Serialize(TSessionObject sessionObject) => JsonConvert.SerializeObject(sessionObject);
}
