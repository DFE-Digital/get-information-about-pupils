using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.Web.Controllers.MyPupilList.PupilSelectionState;
using Moq;

namespace DfE.GIAP.Web.Tests.Controllers.MyPupilList.TestDoubles;
public static class PupilsSelectionStateTestDoubles
{
    public static Mock<IMyPupilsPupilSelectionState> Default() => new();

    public static MyPupilsPupilSelectionState CreateWithPupilUniquePupilNumbers(IEnumerable<string> upns, bool selected = false)
    {
        MyPupilsPupilSelectionState state = new();
        state.AddPupils(upns);
        state.UpdatePupilSelectionState(upns, selected);
        return state;
    }

    public static MyPupilsPupilSelectionState CreateWithSelectedPupils(int count)
    {
        List<string> upns = UniquePupilNumberTestDoubles.Generate(count).Select(t => t.Value).ToList();
        return CreateWithPupilUniquePupilNumbers(upns, selected: true);
    }
}
