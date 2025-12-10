using DfE.GIAP.Web.Features.MyPupils.PresentationService.Models;
using Moq;

namespace DfE.GIAP.Web.Tests.TestDoubles.MyPupils;
public static class MyPupilsPresentationStateTestDoubles
{
    public static MyPupilsPresentationQueryModel Default() => Create(page: 1);

    public static MyPupilsPresentationQueryModel Create(string sortKey)
        => Create(
            sortKey,
            It.IsAny<SortDirection>());

    public static MyPupilsPresentationQueryModel Create(string sortKey, SortDirection sortDirection)
        => Create(
            It.IsAny<int>(),
            sortKey,
            sortDirection);

    public static MyPupilsPresentationQueryModel Create(int page)
        => Create(
                page,
                It.IsAny<string>(),
                It.IsAny<SortDirection>());


    public static MyPupilsPresentationQueryModel Create(int page, string sortKey, SortDirection sortDirection) => new(page, sortKey, sortDirection);
}
