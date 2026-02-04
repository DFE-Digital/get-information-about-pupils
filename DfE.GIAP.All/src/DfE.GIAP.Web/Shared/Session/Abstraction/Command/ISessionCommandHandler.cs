namespace DfE.GIAP.Web.Shared.Session.Abstraction.Command;

public interface ISessionCommandHandler<in TValue> where TValue : class
{
    void StoreInSession(SessionCacheKey sessionCacheKey, TValue value);
}
