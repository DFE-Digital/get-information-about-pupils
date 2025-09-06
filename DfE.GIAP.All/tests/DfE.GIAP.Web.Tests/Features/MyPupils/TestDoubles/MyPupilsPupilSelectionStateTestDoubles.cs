using DfE.GIAP.Web.Features.MyPupils.State.Selection;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.TestDoubles;
public static class MyPupilsPupilSelectionStateTestDoubles
{
    public static MyPupilsPupilSelectionState Default() => new();

    public static MyPupilsPupilSelectionState WithSelectionState(Dictionary<IEnumerable<string>, bool> upnsToSelectionState)
    {
        MyPupilsPupilSelectionState state = Default();
        upnsToSelectionState.ToList().ForEach(mapping =>
        {
            state.UpsertPupilWithSelectedState(mapping.Key, mapping.Value);
        });
        return state;
    }
}
