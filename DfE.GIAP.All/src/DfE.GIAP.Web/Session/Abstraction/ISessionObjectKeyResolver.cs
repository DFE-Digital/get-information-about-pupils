namespace DfE.GIAP.Web.Session.Abstraction;

public interface ISessionObjectKeyResolver
{
    string Resolve<TSessionObject>();
    string Resolve(Type sessionObject);
}
