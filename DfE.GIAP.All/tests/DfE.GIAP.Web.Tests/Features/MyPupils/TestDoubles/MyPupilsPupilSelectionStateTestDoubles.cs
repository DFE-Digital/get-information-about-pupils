using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Web.Features.MyPupils.SelectionState;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.TestDoubles;
public static class MyPupilsPupilSelectionStateTestDoubles
{
    public static MyPupilsPupilSelectionState WithPupilsSelectionState(Dictionary<List<string>, bool> selectionStateMapping)
    {
        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionState.CreateDefault();
        selectionStateMapping.ToList().ForEach(mapping =>
        {
            state.UpsertPupilSelections(mapping.Key, mapping.Value);
        });
        return state;
    }

    public static MyPupilsPupilSelectionState WithAllPupilsSelected(IEnumerable<string> pupils)
    {
        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionState.CreateDefault();
        state.UpsertPupilSelections(pupils, true);
        state.SelectAll();
        return state;
    }
}
