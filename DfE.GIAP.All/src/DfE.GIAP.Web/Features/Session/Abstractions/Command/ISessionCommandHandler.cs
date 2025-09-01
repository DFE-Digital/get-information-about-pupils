using DfE.GIAP.Core.Common.CrossCutting;

namespace DfE.GIAP.Web.Features.Session.Abstractions.Command;

public interface ISessionCommandHandler<in TValue>
{
    void StoreInSession(TValue value);
}
