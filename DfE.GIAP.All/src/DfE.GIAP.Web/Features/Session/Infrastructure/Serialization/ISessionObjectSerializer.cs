namespace DfE.GIAP.Web.Features.Session.Infrastructure.Serialization;

public interface ISessionObjectSerializer<TSessionObject>
{
    string Serialize(TSessionObject sessionObject);
    TSessionObject Deserialize(string input);
}
