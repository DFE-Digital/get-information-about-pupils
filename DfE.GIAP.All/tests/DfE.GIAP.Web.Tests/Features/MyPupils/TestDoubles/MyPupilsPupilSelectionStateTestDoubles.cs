using DfE.GIAP.Web.Features.MyPupils.SelectionState;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.TestDoubles;
public static class MyPupilsPupilSelectionStateTestDoubles
{
    public static MyPupilsPupilSelectionState WithSelectedPupils(
        List<string> selected) => WithSelectedPupils(SelectionMode.Manual, selected, []);

    public static MyPupilsPupilSelectionState WithSelectedPupils(
        SelectionMode mode,
        List<string> selected,
        List<string> deselected)
    {
        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionState.CreateDefault();

        if(mode == SelectionMode.All)
        {
            state.SelectAll();
        }

        foreach (string item in selected)
        {
            state.Select(item);
        }

        foreach (string item in deselected)
        {
            state.Deselect(item);
        }
        return state;
    }
}
