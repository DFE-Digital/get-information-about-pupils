using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;

namespace DfE.GIAP.Web.Tests.TestDoubles.MyPupils;
public static class MyPupilsPupilSelectionStateTestDoubles
{
    public static MyPupilsPupilSelectionState Default() => new();

    public static MyPupilsPupilSelectionState WithPupilsSelectionState(Dictionary<List<string>, bool> selectionStateMapping)
    {
        MyPupilsPupilSelectionState state = Default();
        selectionStateMapping.ToList().ForEach(mapping =>
        {
            state.UpsertPupilSelectionState(mapping.Key, mapping.Value);
        });
        return state;
    }

    public static MyPupilsPupilSelectionState WithAllPupilsSelected(IEnumerable<string> pupils)
    {
        MyPupilsPupilSelectionState state = Default();
        state.UpsertPupilSelectionState(pupils, true);
        state.SelectAllPupils();
        return state;
    }
}
