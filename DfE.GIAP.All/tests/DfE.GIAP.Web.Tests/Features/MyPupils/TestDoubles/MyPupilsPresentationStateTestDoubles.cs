using DfE.GIAP.Web.Features.MyPupils.State.Presentation;
using Moq;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.TestDoubles;
public static class MyPupilsPresentationStateTestDoubles
{
    public static MyPupilsPresentationState CreateWithValidPage() => Create(page: 1);

    public static MyPupilsPresentationState Create(string sortKey)
        => Create(
            sortKey,
            It.IsAny<SortDirection>());

    public static MyPupilsPresentationState Create(string sortKey, SortDirection sortDirection)
        => Create(
            It.IsAny<int>(),
            sortKey,
            sortDirection);

    public static MyPupilsPresentationState Create(int page)
        => Create(
                page,
                It.IsAny<string>(),
                It.IsAny<SortDirection>());


    private static MyPupilsPresentationState Create(int page, string sortKey, SortDirection sortDirection) => new(page, sortKey, sortDirection);
}
