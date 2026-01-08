using DfE.GIAP.Web.Features.MyPupils.PresentationService.Models;
using Moq;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.TestDoubles;
public static class MyPupilsPresentationQueryModelTestDoubles
{
    public static MyPupilsPresentationQueryModel Create(string sortKey)
        => Create(
            sortKey,
            It.IsAny<string>());

    public static MyPupilsPresentationQueryModel Create(string sortKey, string sortDirection)
        => Create(
            1,
            sortKey,
            sortDirection);

    public static MyPupilsPresentationQueryModel Create(int page)
        => Create(
                page,
                It.IsAny<string>(),
                It.IsAny<string>());


    public static MyPupilsPresentationQueryModel Create(int page, string sortKey, string sortDirection)
        => new(
            page,
            pageSize: 20,
            sortKey,
            sortDirection);
}
