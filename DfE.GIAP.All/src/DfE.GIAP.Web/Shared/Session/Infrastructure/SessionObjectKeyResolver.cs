using DfE.GIAP.Web.Shared.Session.Abstraction;

namespace DfE.GIAP.Web.Shared.Session.Infrastructure;

public sealed class SessionObjectKeyResolver : ISessionObjectKeyResolver
{
    public string Resolve<TSessionObject>() => Resolve(typeof(TSessionObject));
    public string Resolve(Type sessionObject) => sessionObject.FullName;
}
