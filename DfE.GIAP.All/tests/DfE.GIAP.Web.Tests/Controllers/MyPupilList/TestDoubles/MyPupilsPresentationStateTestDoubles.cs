using DfE.GIAP.Web.Controllers.MyPupilList.PresentationState;
using Moq;

namespace DfE.GIAP.Web.Tests.Controllers.MyPupilList.TestDoubles;
public static class MyPupilsPresentationStateTestDoubles
{
    public static MyPupilsPresentationState Create(string sortKey) => Create(sortKey, It.IsAny<SortDirection>());

    public static MyPupilsPresentationState Create(string sortKey, SortDirection sortDirection)
    {
        return new(
            Page: It.IsAny<int>(),
            SortBy: sortKey,
            SortDirection: sortDirection);
    }

    public static MyPupilsPresentationState Create(int page)
    {
        return new(
            Page: page,
            SortBy: It.IsAny<string>(),
            SortDirection: It.IsAny<SortDirection>());
    }

    public static MyPupilsPresentationState CreateWithValidPage() => Create(page: 1);

}
