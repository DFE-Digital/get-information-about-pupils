using DfE.GIAP.Web.Features.MyPupils.State.Selection;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.TestDoubles;
public static class MyPupilsPupilSelectionStateTestDoubles
{
    public static MyPupilsPupilSelectionState Default() => new();

    public static MyPupilsPupilSelectionState WithSelectionState(IEnumerable<string> upns, bool selected = false)
    {
        MyPupilsPupilSelectionState state = Default();
        state.UpsertPupilWithSelectedState(upns, selected);
        return state;
    }
}
