using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;

namespace DfE.GIAP.Web.Tests.TestDoubles.MyPupils;
public static class MyPupilsPupilSelectionStateTestDoubles
{
    public static MyPupilsPupilSelectionState Default() => new();

    public static MyPupilsPupilSelectionState WithSelectionState(Dictionary<List<UniquePupilNumber>, bool> selectionStateMapping)
    {
        MyPupilsPupilSelectionState state = Default();
        selectionStateMapping.ToList().ForEach(mapping =>
        {
            state.UpsertUniquePupilNumberSelectionState(mapping.Key, mapping.Value);
        });
        return state;
    }
}
