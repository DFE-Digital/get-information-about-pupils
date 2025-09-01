namespace DfE.GIAP.Web.Features.Session.Abstractions;

public interface ISessionObjectSerializer<TSessionObject>
{
    string Serialize(TSessionObject sessionObject);
    TSessionObject Deserialize(string input);
}
