namespace DfE.GIAP.Web.Shared.Session.Abstraction;

public interface ISessionObjectSerializer<TSessionObject> where TSessionObject : class
{
    string Serialize(TSessionObject sessionObject);
    TSessionObject Deserialize(string input);
}
