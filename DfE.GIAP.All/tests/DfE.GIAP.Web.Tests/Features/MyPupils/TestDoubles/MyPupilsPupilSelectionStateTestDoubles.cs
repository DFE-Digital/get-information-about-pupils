using DfE.GIAP.Web.Features.MyPupils.State.Selection;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.TestDoubles;
public static class MyPupilsPupilSelectionStateTestDoubles
{
    public static MyPupilsPupilSelectionState Default() => new();

    public static MyPupilsPupilSelectionState WithSelectionState(Dictionary<IEnumerable<string>, bool> selectionStateMapping)
    {
        MyPupilsPupilSelectionState state = Default();
        selectionStateMapping.ToList().ForEach(mapping =>
        {
            state.UpsertUniquePupilNumberSelectionState(mapping.Key, mapping.Value);
        });
        return state;
    }
}
