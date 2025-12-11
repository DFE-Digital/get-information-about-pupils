using DfE.GIAP.Web.Features.MyPupils.Areas.UpdateForm;

namespace DfE.GIAP.Web.Features.MyPupils.SelectionState.Command;

public interface IUpdateMyPupilsPupilSelectionsCommandHandler
{
    void Handle(MyPupilsFormStateRequestDto request);
}
