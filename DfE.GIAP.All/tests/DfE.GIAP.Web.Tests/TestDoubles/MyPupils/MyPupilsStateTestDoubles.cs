using DfE.GIAP.Web.Features.MyPupils.PresentationService.Models;
using DfE.GIAP.Web.Features.MyPupils.State.Models;
using DfE.GIAP.Web.Features.MyPupils.State.Models.Selection;

namespace DfE.GIAP.Web.Tests.TestDoubles.MyPupils;
internal static class MyPupilsStateTestDoubles
{
    internal static MyPupilsState Create(MyPupilsPresentationQueryModel presentationState, MyPupilsPupilSelectionState selectionState)
        => new(presentationState, selectionState);

    internal static MyPupilsState Default()
    => Create(
        MyPupilsPresentationStateTestDoubles.Default(),
        MyPupilsPupilSelectionStateTestDoubles.Default());
}
