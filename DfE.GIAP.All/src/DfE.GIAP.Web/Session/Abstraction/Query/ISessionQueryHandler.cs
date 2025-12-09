namespace DfE.GIAP.Web.Session.Abstraction.Query;

public interface ISessionQueryHandler<TSessionObject> where TSessionObject : class
{
    SessionQueryResponse<TSessionObject> Handle();
}
