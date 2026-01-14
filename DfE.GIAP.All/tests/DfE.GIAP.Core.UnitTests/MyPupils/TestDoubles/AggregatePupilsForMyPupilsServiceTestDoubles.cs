using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;
internal static class AggregatePupilsForMyPupilsServiceTestDoubles
{
    internal static Mock<IAggregatePupilsForMyPupilsApplicationService> Default() => new();

    internal static Mock<IAggregatePupilsForMyPupilsApplicationService> MockFor(IEnumerable<Pupil> pupils)
    {
        Mock<IAggregatePupilsForMyPupilsApplicationService> mockService = Default();

        mockService.Setup(
                (service) => service.GetPupilsAsync(It.IsAny<UniquePupilNumbers>()))
            .ReturnsAsync(pupils)
            .Verifiable();

        return mockService;
    }
}
