using DfE.GIAP.Web.Features.MyPupils.State.Presentation;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;

namespace DfE.GIAP.Web.Features.MyPupils.State;

public record MyPupilsState(
    MyPupilsPresentationState PresentationState,
    MyPupilsPupilSelectionState SelectionState);
