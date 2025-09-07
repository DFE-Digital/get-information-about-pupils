using DfE.GIAP.Web.Features.MyPupils.GetPaginatedMyPupils;
using Moq;

namespace DfE.GIAP.Web.Tests.TestDoubles.MyPupils;
internal static class IGetPaginatedMyPupilsHandlerTestDoubles
{
    internal static Mock<IGetPaginatedMyPupilsHandler> Default() => new();

    internal static Mock<IGetPaginatedMyPupilsHandler> MockFor(PaginatedMyPupilsResponse response)
    {
        Mock<IGetPaginatedMyPupilsHandler> mock = Default();
        mock.Setup(
                (handler) => handler.HandleAsync(
                    It.IsAny<GetPaginatedMyPupilsRequest>()))
            .ReturnsAsync(response)
            .Verifiable();

        return mock;
    }
}
