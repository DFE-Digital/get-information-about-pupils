using DfE.GIAP.Web.Features.MyPupils.SelectionState;
using DfE.GIAP.Web.Features.MyPupils.State.Presentation;

namespace DfE.GIAP.Web.Features.MyPupils.State;

public record MyPupilsState(
    MyPupilsPresentationState PresentationState,
    MyPupilsPupilSelectionState SelectionState);

