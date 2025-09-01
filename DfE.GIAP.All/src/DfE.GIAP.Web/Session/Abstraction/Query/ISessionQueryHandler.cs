namespace DfE.GIAP.Web.Session.Abstraction.Query;

public interface ISessionQueryHandler<TSessionObject>
{
    SessionQueryResponse<TSessionObject> GetSessionObject();
}
