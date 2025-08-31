namespace DfE.GIAP.Web.Features.Session.Infrastructure.KeyResolver;
public interface ISessionObjectKeyResolver
{
    string Resolve<TSessionObject>();
    string Resolve(Type sessionObject);
}
