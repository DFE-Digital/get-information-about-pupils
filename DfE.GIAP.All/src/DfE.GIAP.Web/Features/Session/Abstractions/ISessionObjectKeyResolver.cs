namespace DfE.GIAP.Web.Features.Session.Abstractions;
public interface ISessionObjectKeyResolver
{
    string Resolve<TSessionObject>();
    string Resolve(Type sessionObject);
}
