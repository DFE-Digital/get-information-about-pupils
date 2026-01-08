using DfE.GIAP.Web.Features.MyPupils.Areas.UpdateForm;

namespace DfE.GIAP.Web.Features.MyPupils.SelectionState.UpdatePupilSelections;

public interface IUpdateMyPupilsPupilSelectionsCommandHandler
{
    void Handle(MyPupilsFormStateRequestDto request);
}
