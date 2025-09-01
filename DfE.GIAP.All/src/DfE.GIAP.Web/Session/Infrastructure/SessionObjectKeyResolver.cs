using DfE.GIAP.Web.Session.Abstraction;

namespace DfE.GIAP.Web.Session.Infrastructure;

public sealed class SessionObjectKeyResolver : ISessionObjectKeyResolver
{
    public string Resolve<TSessionObject>() => Resolve(typeof(TSessionObject));
    public string Resolve(Type sessionObject) => sessionObject.FullName;
}
