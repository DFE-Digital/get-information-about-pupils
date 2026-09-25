namespace DfE.GIAP.Web.Shared.Session.Abstraction;

public interface ISessionObjectSerializer<TSessionObject> where TSessionObject : class
{
    // TODO extend the serialiser to support byte[]
    string Serialize(TSessionObject sessionObject);
    TSessionObject Deserialize(string input);
}
