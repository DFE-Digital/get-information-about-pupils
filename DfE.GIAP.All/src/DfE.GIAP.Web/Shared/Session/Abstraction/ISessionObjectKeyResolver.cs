namespace DfE.GIAP.Web.Shared.Session.Abstraction;

public interface ISessionObjectKeyResolver
{
    string Resolve<TSessionObject>();
    string Resolve(Type sessionObject);
}
