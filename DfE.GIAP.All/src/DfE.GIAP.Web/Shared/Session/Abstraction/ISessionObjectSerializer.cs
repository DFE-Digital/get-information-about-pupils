namespace DfE.GIAP.Web.Shared.Session.Abstraction;

public interface ISessionObjectSerializer<TSessionObject>
{
    string Serialize(TSessionObject sessionObject);
    TSessionObject Deserialize(string input);
}
