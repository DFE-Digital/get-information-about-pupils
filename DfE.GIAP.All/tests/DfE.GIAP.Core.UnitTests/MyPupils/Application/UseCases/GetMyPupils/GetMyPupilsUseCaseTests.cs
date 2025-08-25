using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;
using DfE.GIAP.Core.UnitTests.TestDoubles;
using DfE.GIAP.Core.User.Application.Repository;
using DfE.GIAP.SharedTests.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Application.UseCases.GetMyPupils;
public sealed class GetMyPupilsUseCaseTests
{
    [Fact]
    public async Task HandleRequestAsync_ReturnsMappedPupils()
    {
        // Arrange
        User.Application.User user = UserTestDoubles.Default();
        Mock<IUserReadOnlyRepository> userRepoMock = UserReadOnlyRepositoryTestDoubles.MockForGetUserById(user);

        List<Pupil> pupils =
            user.UniquePupilNumbers
                .Select((upn) => PupilBuilder.CreateBuilder(upn).Build())
                .ToList();

        Mock<IAggregatePupilsForMyPupilsApplicationService> aggregateServiceMock = AggregatePupilsForMyPupilsServiceTestDoubles.MockFor(pupils);

        Mock<IMapper<Pupil, PupilDto>> mockMapper = MapperTestDoubles.Default<Pupil, PupilDto>();
        List<PupilDto> pupilDtos = PupilDtoTestDoubles.GenerateWithUniquePupilNumbers(pupils.Select(t => t.Identifier));
        mockMapper.MockForMany(pupils, pupilDtos);

        GetMyPupilsRequest request = new(user.UserId.Value);

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
            repo.GetUserByIdAsync(user.UserId, It.IsAny<CancellationToken>()), Times.Once);

        aggregateServiceMock.Verify(
            t => t.GetPupilsAsync(user.UniquePupilNumbers), Times.Once);

        mockMapper.Verify(
            t => t.Map(It.IsAny<Pupil>()), Times.Exactly(pupils.Count));
    }

    [Fact]
    public async Task HandleRequestAsync_WithEmptyPupilList_ReturnsEmptyResponse()
    {
        // Arrange
        User.Application.User user = UserTestDoubles.WithEmptyUpns();

        Mock<IUserReadOnlyRepository> userRepoMock = UserReadOnlyRepositoryTestDoubles.MockForGetUserById(user);

        Mock<IAggregatePupilsForMyPupilsApplicationService> mockAggregateService = AggregatePupilsForMyPupilsServiceTestDoubles.Default();

        Mock<IMapper<Pupil, PupilDto>> mockMapper = MapperTestDoubles.Default<Pupil, PupilDto>();

        GetMyPupilsRequest request = new(user.UserId.Value);

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
            repo.GetUserByIdAsync(
                user.UserId,
                It.IsAny<CancellationToken>()), Times.Once);

        mockAggregateService.Verify(
            t => t.GetPupilsAsync(Enumerable.Empty<UniquePupilNumber>()), Times.Never);

        mockMapper.Verify(t => t.Map(It.IsAny<Pupil>()), Times.Never);

    }
}
