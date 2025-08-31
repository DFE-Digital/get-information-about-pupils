namespace DfE.GIAP.Web.Features.Session.Infrastructure.KeyResolver;

public sealed class SessionObjectKeyResolver : ISessionObjectKeyResolver
{
    public string Resolve<TSessionObject>() => Resolve(typeof(TSessionObject));
    public string Resolve(Type sessionObject) => sessionObject.FullName;
}
