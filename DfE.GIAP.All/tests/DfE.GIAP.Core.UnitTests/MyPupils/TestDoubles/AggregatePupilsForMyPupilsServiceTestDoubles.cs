using DfE.GIAP.Core.Common.Domain;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.QueryModel;
using DfE.GIAP.Core.MyPupils.Domain.Entities;

namespace DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;
internal static class AggregatePupilsForMyPupilsServiceTestDoubles
{
    internal static Mock<IAggregatePupilsForMyPupilsApplicationService> Default() => new();

    internal static Mock<IAggregatePupilsForMyPupilsApplicationService> MockFor(IEnumerable<Pupil> pupils)
    {
        Mock<IAggregatePupilsForMyPupilsApplicationService> mockService = Default();

        mockService.Setup(
                (service) => service.GetPupilsAsync(
                    It.IsAny<UniquePupilNumbers>(), It.IsAny<MyPupilsQueryModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pupils)
            .Verifiable();

        return mockService;
    }
}
