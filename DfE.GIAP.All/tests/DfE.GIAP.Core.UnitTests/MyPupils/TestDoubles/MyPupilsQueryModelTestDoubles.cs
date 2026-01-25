using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.QueryModel;

namespace DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;
public static class MyPupilsQueryModelTestDoubles
{
    public static MyPupilsQueryModel Create(string sortKey)
        => Create(
            sortKey,
            It.IsAny<string>());

    public static MyPupilsQueryModel Create(string sortKey, string sortDirection)
        => Create(
            1,
            sortKey,
            sortDirection);

    public static MyPupilsQueryModel Create(int page)
        => Create(
                page,
                It.IsAny<string>(),
                It.IsAny<string>());


    public static MyPupilsQueryModel Create(int page, string sortKey, string sortDirection)
        => new(
            page,
            size: 20,
            (sortKey, sortDirection));
}
