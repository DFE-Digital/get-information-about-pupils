using DfE.GIAP.Web.Features.MyPupils.PresentationState;
using DfE.GIAP.Web.Features.MyPupils.SelectionState;

namespace DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils;

public record GetMyPupilsRequest(
    string UserId,
    MyPupilsPresentationState PresentationState,
    MyPupilsPupilSelectionState SelectionState);
