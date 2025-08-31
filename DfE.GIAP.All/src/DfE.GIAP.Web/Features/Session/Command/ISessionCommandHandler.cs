using DfE.GIAP.Core.Common.CrossCutting;

namespace DfE.GIAP.Web.Features.Session.Command;

public interface ISessionCommandHandler<TValue>
{
    void StoreInSession(TValue value);
    void StoreInSession<TDataTransferObject>(
        TValue value,
        IMapper<TValue, TDataTransferObject> mapSessionObjectToDto);
}
