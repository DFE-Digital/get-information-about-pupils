using DfE.GIAP.Web.Features.MyPupils.Areas.UpdateForm;

namespace DfE.GIAP.Web.Features.MyPupils.State;

public interface IUpdateMyPupilsPupilSelectionsCommandHandler
{
    void Handle(MyPupilsFormStateRequestDto request);
}
