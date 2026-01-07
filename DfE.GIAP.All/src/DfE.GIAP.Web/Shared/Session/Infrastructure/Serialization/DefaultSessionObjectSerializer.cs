using DfE.GIAP.Web.Shared.Session.Abstraction;
using Newtonsoft.Json;

namespace DfE.GIAP.Web.Shared.Session.Infrastructure.Serialization;

public class DefaultSessionObjectSerializer<TSessionObject> : ISessionObjectSerializer<TSessionObject>
{
    public TSessionObject Deserialize(string input) => JsonConvert.DeserializeObject<TSessionObject>(input);
    public string Serialize(TSessionObject sessionObject) => JsonConvert.SerializeObject(sessionObject);
}
