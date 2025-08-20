using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PresenterHandler.Options;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PresenterHandler.Order;
using Moq;

namespace DfE.GIAP.Web.Tests.Controllers.MyPupils.TestDoubles;
public static class PupilPresentationOptionsTestDoubles
{
    public static PupilsPresentationOptions Create(string sortKey) => Create(sortKey, It.IsAny<SortDirection>());

    public static PupilsPresentationOptions Create(string sortKey, SortDirection sortDirection)
    {
        return new(
            Page: It.IsAny<int>(),
            SortBy: sortKey,
            SortDirection: sortDirection);
    }

    public static PupilsPresentationOptions Create(int page)
    {
        return new(
            Page: page,
            SortBy: It.IsAny<string>(),
            SortDirection: It.IsAny<SortDirection>());
    }

    public static PupilsPresentationOptions CreateWithValidPage() => Create(page: 1);

}
