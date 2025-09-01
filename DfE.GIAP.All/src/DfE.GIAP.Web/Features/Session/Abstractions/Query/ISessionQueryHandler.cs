using DfE.GIAP.Web.Providers.Session;

namespace DfE.GIAP.Web.Features.Session.Abstractions.Query;

public interface ISessionQueryHandler<TSessionObject>
{
    SessionQueryResponse<TSessionObject> GetSessionObject();
}
