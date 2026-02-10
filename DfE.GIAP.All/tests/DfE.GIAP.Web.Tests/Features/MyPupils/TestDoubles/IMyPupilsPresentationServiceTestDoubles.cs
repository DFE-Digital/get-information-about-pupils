using DfE.GIAP.Web.Features.MyPupils.Controllers;
using DfE.GIAP.Web.Features.MyPupils.Services.GetPupils;
using Moq;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.TestDoubles;
internal static class IMyPupilsPresentationServiceTestDoubles
{
    internal static Mock<IGetMyPupilsPresentationService> DefaultMock() => new();

    internal static IGetMyPupilsPresentationService MockForGetPupils(MyPupilsPresentationResponse? stub)
    {
        Mock<IGetMyPupilsPresentationService> mock = DefaultMock();
        mock.Setup(
            (t) => t.GetPupilsAsync(
                It.IsAny<string>(),
                It.IsAny<MyPupilsQueryRequestDto>()))
            .ReturnsAsync(stub);

        return mock.Object;
    }
}
