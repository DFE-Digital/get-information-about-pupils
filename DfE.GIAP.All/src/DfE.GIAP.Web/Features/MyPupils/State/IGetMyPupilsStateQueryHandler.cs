using DfE.GIAP.Web.Features.MyPupils.State.Models;

namespace DfE.GIAP.Web.Features.MyPupils.State;

public interface IGetMyPupilsStateQueryHandler
{
    MyPupilsState GetState();
}
