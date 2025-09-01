using DfE.GIAP.Web.Session.Abstraction;
using Newtonsoft.Json;

namespace DfE.GIAP.Web.Session.Infrastructure.Serialization;

public class DefaultSessionObjectSerializer<TSessionObject> : ISessionObjectSerializer<TSessionObject>
{
    public TSessionObject Deserialize(string input) => JsonConvert.DeserializeObject<TSessionObject>(input);
    public string Serialize(TSessionObject sessionObject) => JsonConvert.SerializeObject(sessionObject);
}
