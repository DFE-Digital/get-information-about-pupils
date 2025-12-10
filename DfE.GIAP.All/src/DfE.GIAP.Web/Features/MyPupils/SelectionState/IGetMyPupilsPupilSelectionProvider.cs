using DfE.GIAP.Web.Features.MyPupils.State.Models;
using DfE.GIAP.Web.Features.MyPupils.State.Models.Selection;

namespace DfE.GIAP.Web.Features.MyPupils.State;

public interface IGetMyPupilsPupilSelectionProvider
{
    MyPupilsPupilSelectionState GetPupilSelections();
}
