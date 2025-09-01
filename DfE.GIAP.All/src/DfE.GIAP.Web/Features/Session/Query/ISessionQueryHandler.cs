using DfE.GIAP.Web.Providers.Session;

namespace DfE.GIAP.Web.Features.Session.Query;

public interface ISessionQueryHandler<TSessionObject>
{
    SessionQueryResponse<TSessionObject> GetSessionObject();
}
