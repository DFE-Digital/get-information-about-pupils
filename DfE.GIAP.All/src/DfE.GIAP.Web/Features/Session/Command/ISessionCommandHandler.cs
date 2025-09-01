using DfE.GIAP.Core.Common.CrossCutting;

namespace DfE.GIAP.Web.Features.Session.Command;

public interface ISessionCommandHandler<in TValue>
{
    void StoreInSession(TValue value);
}
