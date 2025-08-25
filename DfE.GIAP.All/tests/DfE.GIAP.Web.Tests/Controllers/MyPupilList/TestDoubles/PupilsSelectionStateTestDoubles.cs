using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.Web.Controllers.MyPupilList.PupilSelectionState;
using Moq;

namespace DfE.GIAP.Web.Tests.Controllers.MyPupilList.TestDoubles;
public static class PupilsSelectionStateTestDoubles
{
    public static Mock<IPupilsSelectionState> Default() => new();

    public static PupilsSelectionState CreateWithPupilUniquePupilNumbers(IEnumerable<string> upns, bool selected = false)
    {
        PupilsSelectionState state = new();
        state.AddPupils(upns);
        state.UpdatePupilSelectionState(upns, selected);
        return state;
    }

    public static PupilsSelectionState CreateWithSelectedPupils(int count)
    {
        List<string> upns = UniquePupilNumberTestDoubles.Generate(count).Select(t => t.Value).ToList();
        return CreateWithPupilUniquePupilNumbers(upns, selected: true);
    }
}
