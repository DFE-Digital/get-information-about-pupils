namespace DfE.GIAP.Web.Session.Abstraction;

public interface ISessionObjectSerializer<TSessionObject>
{
    string Serialize(TSessionObject sessionObject);
    TSessionObject Deserialize(string input);
}
