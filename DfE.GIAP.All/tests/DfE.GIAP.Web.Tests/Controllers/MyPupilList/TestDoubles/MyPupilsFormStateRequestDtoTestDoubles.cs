using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.Web.Controllers.MyPupilList;

namespace DfE.GIAP.Web.Tests.Controllers.MyPupilList.TestDoubles;
internal static class MyPupilsFormStateRequestDtoTestDoubles
{
    internal static MyPupilsFormStateRequestDto CreateWithSelectionState(bool selectAll)
    {
        return new()
        {
            SelectAll = selectAll,
            CurrentPageOfPupils = string.Join(',', UniquePupilNumberTestDoubles.GenerateAsValues(20))
        };
    }

    internal static MyPupilsFormStateRequestDto CreateWithSelectedPupils(
        IEnumerable<UniquePupilNumber> allPupilsOnPage,
        IEnumerable<UniquePupilNumber> selectedPupilsOnPage)
    {
        return new()
        {
            SelectedPupils = selectedPupilsOnPage.Select(t => t.Value).ToList(),
            CurrentPageOfPupils = string.Join(',', allPupilsOnPage),
        };
    }
}
