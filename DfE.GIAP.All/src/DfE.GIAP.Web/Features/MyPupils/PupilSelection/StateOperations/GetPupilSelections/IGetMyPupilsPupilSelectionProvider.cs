using DfE.GIAP.Web.Features.MyPupils.PupilSelection.Operations;

namespace DfE.GIAP.Web.Features.MyPupils.PupilSelection.Operations.GetPupilSelections;

public interface IGetMyPupilsPupilSelectionProvider
{
    MyPupilsPupilSelectionState GetPupilSelections();
}
