using DfE.GIAP.Web.Features.MyPupils.State;
using DfE.GIAP.Web.Features.MyPupils.State.Presentation;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;

namespace DfE.GIAP.Web.Tests.TestDoubles.MyPupils;
internal static class MyPupilsStateTestDoubles
{
    internal static MyPupilsState Create(MyPupilsPresentationState presentationState, MyPupilsPupilSelectionState selectionState)
        => new(presentationState, selectionState);

    internal static MyPupilsState Default()
    => Create(
        MyPupilsPresentationStateTestDoubles.Default(),
        MyPupilsPupilSelectionStateTestDoubles.Default());
}
