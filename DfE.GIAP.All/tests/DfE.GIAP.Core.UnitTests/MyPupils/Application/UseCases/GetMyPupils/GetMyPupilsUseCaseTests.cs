using Bogus;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.Services;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.SharedTests.TestDoubles;
using DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;
using DfE.GIAP.Core.UnitTests.TestDoubles;
using DfE.GIAP.Core.User.Application;
using DfE.GIAP.Core.User.Application.Repository;
using DfE.GIAP.SharedTests.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Application.UseCases.GetMyPupils;
public sealed class GetMyPupilsUseCaseTests
{
    [Fact]
    public async Task HandleRequestAsync_ReturnsMappedPupils()
    {
        // Arrange
        UserId userId = UserIdTestDoubles.Default();
        GetMyPupilsRequest request = new(userId.Value);
        List<UniquePupilNumber> upns = UniquePupilNumberTestDoubles.Generate(count: 3);
        User.Application.User user = new(userId, upns);

        Mock<IUserReadOnlyRepository> userRepoMock = UserReadOnlyRepositoryTestDoubles.MockForGetUserById(user, userId);

        List<Pupil> pupils =
            upns.Select((upn) => PupilBuilder.CreateBuilder(upn).Build())
                .ToList();

        Mock<IAggregatePupilsForMyPupilsApplicationService> aggregateServiceMock = AggregatePupilsForMyPupilsServiceTestDoubles.MockFor(pupils, upns);

        Mock<IMapper<Pupil, PupilDto>> mockMapper = MapperTestDoubles.Default<Pupil, PupilDto>();
        List<PupilDto> pupilDtos = PupilDtoTestDoubles.GenerateWithUniquePupilNumbers(pupils.Select(t => t.Identifier));
        for (int index = 0; index < pupils.Count; index++)
        {
            mockMapper
                .Setup(m => m.Map(pupils[index]))
                .Returns(pupilDtos[index])
                .Verifiable();
        }

        // Act
        GetMyPupilsUseCase sut = new(
            userRepoMock.Object,
            aggregateServiceMock.Object,
            mockMapper.Object);

        GetMyPupilsResponse result = await sut.HandleRequestAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(pupilDtos.Count, result.Pupils.Count());
        Assert.Equivalent(result.Pupils, pupilDtos);

        userRepoMock.Verify(repo =>
            repo.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);

        aggregateServiceMock.Verify(
            t => t.GetPupilsAsync(upns, It.IsAny<MyPupilsQueryOptions>()), Times.Once);

        mockMapper.Verify(
            t => t.Map(It.IsAny<Pupil>()), Times.Exactly(pupils.Count));
    }

    [Fact]
    public async Task HandleRequestAsync_WithEmptyPupilList_ReturnsEmptyResponse()
    {
        // Arrange
        UserId userId = UserIdTestDoubles.Default();
        GetMyPupilsRequest request = new(userId.Value);
        User.Application.User user = new(userId, []);

        Mock<IUserReadOnlyRepository> userRepoMock = UserReadOnlyRepositoryTestDoubles.MockForGetUserById(user, userId);

        Mock<IAggregatePupilsForMyPupilsApplicationService> mockAggregateService = AggregatePupilsForMyPupilsServiceTestDoubles.Default();

        Mock<IMapper<Pupil, PupilDto>> mockMapper = MapperTestDoubles.Default<Pupil, PupilDto>();

        // Act
        GetMyPupilsUseCase sut = new(
            userRepoMock.Object,
            mockAggregateService.Object,
            mockMapper.Object);

        GetMyPupilsResponse result = await sut.HandleRequestAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Pupils);

        userRepoMock.Verify(repo =>
            repo.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);

        mockAggregateService.Verify(
            t => t.GetPupilsAsync(Enumerable.Empty<UniquePupilNumber>(), It.IsAny<MyPupilsQueryOptions>()), Times.Never);

        mockMapper.Verify(t => t.Map(It.IsAny<Pupil>()), Times.Never);

    }
}
