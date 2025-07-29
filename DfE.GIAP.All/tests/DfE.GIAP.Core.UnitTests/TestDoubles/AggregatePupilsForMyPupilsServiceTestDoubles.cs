using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DfE.GIAP.Core.MyPupils.Application.Services;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.UnitTests.TestDoubles;
internal static class AggregatePupilsForMyPupilsServiceTestDoubles
{
    internal static Mock<IAggregatePupilsForMyPupilsApplicationService> Default() => new();

    internal static Mock<IAggregatePupilsForMyPupilsApplicationService> MockFor(IEnumerable<Pupil> pupils, IEnumerable<UniquePupilNumber> matchUpns)
    {
        Mock<IAggregatePupilsForMyPupilsApplicationService> mockService = Default();

        mockService.Setup(
                (service) => service.GetPupilsAsync(matchUpns, It.IsAny<MyPupilsQueryOptions>()))
            .ReturnsAsync(pupils)
            .Verifiable();

        return mockService;
    }
}
