using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils;
using DfE.GIAP.Core.MyPupils.Domain.Entities;

namespace DfE.GIAP.Core.UnitTests.TestDoubles;
internal static class AggregatePupilsForMyPupilsServiceTestDoubles
{
    internal static Mock<IAggregatePupilsForMyPupilsApplicationService> Default() => new();

    internal static Mock<IAggregatePupilsForMyPupilsApplicationService> MockFor(IEnumerable<Pupil> pupils)
    {
        Mock<IAggregatePupilsForMyPupilsApplicationService> mockService = Default();

        mockService.Setup(
                (service) => service.GetPupilsAsync(matchUpns))
            .ReturnsAsync(pupils)
            .Verifiable();

        return mockService;
    }
}
