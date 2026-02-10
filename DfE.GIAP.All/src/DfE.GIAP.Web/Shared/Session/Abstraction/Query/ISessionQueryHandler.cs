namespace DfE.GIAP.Web.Shared.Session.Abstraction.Query;

public interface ISessionQueryHandler<TSessionObject> where TSessionObject : class
{
    SessionQueryResponse<TSessionObject> Handle(SessionCacheKey key);
}
