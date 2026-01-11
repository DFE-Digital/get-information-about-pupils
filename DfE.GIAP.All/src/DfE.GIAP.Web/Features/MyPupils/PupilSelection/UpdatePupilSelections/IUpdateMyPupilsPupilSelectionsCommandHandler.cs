using DfE.GIAP.Web.Features.MyPupils.Controllers.UpdateForm;

namespace DfE.GIAP.Web.Features.MyPupils.PupilSelection.UpdatePupilSelections;

public interface IUpdateMyPupilsPupilSelectionsCommandHandler
{
    void Handle(MyPupilsFormStateRequestDto request);
}