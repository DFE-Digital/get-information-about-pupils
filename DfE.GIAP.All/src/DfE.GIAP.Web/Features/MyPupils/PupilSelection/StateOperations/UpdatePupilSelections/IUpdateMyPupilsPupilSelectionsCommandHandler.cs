using DfE.GIAP.Web.Features.MyPupils.Areas.UpdateForm;

namespace DfE.GIAP.Web.Features.MyPupils.PupilSelection.Operations.UpdatePupilSelections;

public interface IUpdateMyPupilsPupilSelectionsCommandHandler
{
    Task Handle(MyPupilsFormStateRequestDto request);
}
