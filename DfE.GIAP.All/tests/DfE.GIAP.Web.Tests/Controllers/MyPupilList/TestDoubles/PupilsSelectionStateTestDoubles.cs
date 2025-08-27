using DfE.GIAP.Web.Features.MyPupils.SelectionState;

namespace DfE.GIAP.Web.Tests.Controllers.MyPupilList.TestDoubles;
public static class PupilsSelectionStateTestDoubles
{
    public static MyPupilsPupilSelectionState CreateEmpty()
    {
        return new MyPupilsPupilSelectionState();
    }

    public static MyPupilsPupilSelectionState CreateWithSelectionState(IEnumerable<string> upns, bool selected = false)
    {
        MyPupilsPupilSelectionState state = new();
        state.UpsertPupilSelectionState(upns, selected);
        return state;
    }
}
