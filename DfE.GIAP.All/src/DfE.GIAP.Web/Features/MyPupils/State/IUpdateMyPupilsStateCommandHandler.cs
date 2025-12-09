using DfE.GIAP.Web.Features.MyPupils.Areas.UpdateForm;

namespace DfE.GIAP.Web.Features.MyPupils.State;

public interface IUpdateMyPupilsStateCommandHandler
{
    void Handle(MyPupilsFormStateRequestDto request);
}
