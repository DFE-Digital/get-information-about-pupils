using DfE.GIAP.Web.Features.MyPupils.PresentationService.Models;
using DfE.GIAP.Web.Features.MyPupils.SelectionState;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.TestDoubles;
internal static class MyPupilsStateTestDoubles
{
    internal static MyPupilsState Default() => MyPupilsState.Create(
        MyPupilsPresentationQueryModel.CreateDefault(),
        MyPupilsPupilSelectionState.CreateDefault());
}
