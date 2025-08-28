using DfE.GIAP.Web.Features.MyPupils.SelectionState;

namespace DfE.GIAP.Web.Tests.Controllers.MyPupilList.TestDoubles;
public static class MyPupilsPupilSelectionStateTestDoubles
{
    public static MyPupilsPupilSelectionState CreateEmpty()
    {
        return new MyPupilsPupilSelectionState();
    }

    public static MyPupilsPupilSelectionState CreateWithSelectionState(IEnumerable<string> upns, bool selected = false)
    {
        MyPupilsPupilSelectionState state = new();
        state.UpsertPupilWithSelectedState(upns, selected);
        return state;
    }
}
