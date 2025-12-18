using DfE.GIAP.Web.Features.MyPupils.Controllers;
using DfE.GIAP.Web.Features.MyPupils.PresentationService;
using Moq;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.TestDoubles;
internal static class IMyPupilsPresentationServiceTestDoubles
{
    internal static Mock<IMyPupilsPresentationService> DefaultMock() => new();

    internal static IMyPupilsPresentationService MockForGetPupils(MyPupilsPresentationResponse? stub)
    {
        Mock<IMyPupilsPresentationService> mock = DefaultMock();
        mock.Setup(
            (t) => t.GetPupils(
                It.IsAny<string>(),
                It.IsAny<MyPupilsQueryRequestDto>()))
            .ReturnsAsync(stub);

        return mock.Object;
    }

    internal static IMyPupilsPresentationService MockForGetSelectedPupils(IEnumerable<string> stub)
    {
        Mock<IMyPupilsPresentationService> mock = DefaultMock();
        mock.Setup(
            (t) => t.GetSelectedPupilUniquePupilNumbers(
                It.IsAny<string>()))
            .ReturnsAsync(stub);

        return mock.Object;
    }
}
