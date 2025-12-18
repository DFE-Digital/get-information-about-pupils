using DfE.GIAP.Web.Features.MyPupils.PresentationService.Models;
using Moq;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.TestDoubles;
public static class MyPupilsPresentationQueyTestDoubles
{

    public static MyPupilsPresentationQueryModel Create(string sortKey)
        => Create(
            sortKey,
            It.IsAny<string>());

    public static MyPupilsPresentationQueryModel Create(string sortKey, string sortDirection)
        => Create(
            It.IsAny<int>(),
            sortKey,
            sortDirection);

    public static MyPupilsPresentationQueryModel Create(int page)
        => Create(
                page,
                It.IsAny<string>(),
                It.IsAny<string>());


    public static MyPupilsPresentationQueryModel Create(int page, string sortKey, string sortDirection)
        => new(page, sortKey, sortDirection);
}
