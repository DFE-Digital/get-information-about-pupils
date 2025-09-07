using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.Repositories;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Services.AggregatePupilsForMyPupils;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.SharedTests.TestDoubles;
using DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;
using DfE.GIAP.Core.UnitTests.TestDoubles;
using DfE.GIAP.Core.Users.Application;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles.MyPupils;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Application.UseCases.GetMyPupils;
public sealed class GetMyPupilsUseCaseTests
{
    [Fact]
    public async Task HandleRequestAsync_ReturnsMappedPupils()
    {
        // Arrange
        UserId userId = UserIdTestDoubles.Default();

        Core.MyPupils.Application.Repositories.MyPupils myPupils = MyPupilsTestDoubles.Default();
        Mock<IMyPupilsReadOnlyRepository> readRepositoryMock = IMyPupilsReadOnlyRepositoryTestDoubles.MockFor(myPupils);

        List<Pupil> pupils =
            myPupils.Pupils
                .GetUniquePupilNumbers()
                .Select((upn) => PupilBuilder.CreateBuilder(upn).Build())
                .ToList();

        Mock<IAggregatePupilsForMyPupilsApplicationService> aggregateServiceMock = AggregatePupilsForMyPupilsServiceTestDoubles.MockFor(pupils);

        Mock<IMapper<Pupil, MyPupilDto>> mapperMock = MapperTestDoubles.Default<Pupil, MyPupilDto>();
        MyPupilDtos myPupilDtos = MyPupilDtosTestDoubles.GenerateWithUniquePupilNumbers(pupils.Select(t => t.Identifier));

        mapperMock.MockMappingForMany(pupils, myPupilDtos.Values.ToList());

        GetMyPupilsRequest request = new(userId);

        // Act
        GetMyPupilsUseCase sut = new(
            readRepositoryMock.Object,
            aggregateServiceMock.Object,
            mapperMock.Object);

        GetMyPupilsResponse result = await sut.HandleRequestAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(myPupilDtos.Count, result.MyPupils.Count);
        Assert.Equivalent(result.MyPupils, myPupilDtos);

        readRepositoryMock.Verify(repo =>
            repo.GetMyPupilsOrDefaultAsync(userId, It.IsAny<CancellationToken>()), Times.Once);

        aggregateServiceMock.Verify(
            t => t.GetPupilsAsync(myPupils.Pupils), Times.Once);

        mapperMock.Verify(
            t => t.Map(It.IsAny<Pupil>()), Times.Exactly(pupils.Count));
    }

    [Fact]
    public async Task HandleRequestAsync_WithNullRepositoryResponse_ReturnsEmptyResponse()
    {
        // Arrange
        UserId userId = UserIdTestDoubles.Default();

        Mock<IMyPupilsReadOnlyRepository> userRepoMock = IMyPupilsReadOnlyRepositoryTestDoubles.MockFor(null!);

        Mock<IAggregatePupilsForMyPupilsApplicationService> mockAggregateService = AggregatePupilsForMyPupilsServiceTestDoubles.Default();

        Mock<IMapper<Pupil, MyPupilDto>> mockMapper = MapperTestDoubles.Default<Pupil, MyPupilDto>();

        GetMyPupilsRequest request = new(userId);

        // Act
        GetMyPupilsUseCase sut = new(
            userRepoMock.Object,
            mockAggregateService.Object,
            mockMapper.Object);

        GetMyPupilsResponse result = await sut.HandleRequestAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.MyPupils.Values);

        userRepoMock.Verify(repo =>
            repo.GetMyPupilsOrDefaultAsync(
                userId,
                It.IsAny<CancellationToken>()), Times.Once);

        mockAggregateService.Verify(
            t => t.GetPupilsAsync(It.Is<UniquePupilNumbers>(t => !t.GetUniquePupilNumbers().Any())), Times.Never);

        mockMapper.Verify(t => t.Map(It.IsAny<Pupil>()), Times.Never);
    }

    [Fact]
    public async Task HandleRequestAsync_WithEmptyPupilList_ReturnsEmptyResponse()
    {
        // Arrange
        UserId userId = UserIdTestDoubles.Default();

        Core.MyPupils.Application.Repositories.MyPupils myPupils =
            MyPupilsTestDoubles.Create(
                UniquePupilNumbers.Create(uniquePupilNumbers: []));

        Mock<IMyPupilsReadOnlyRepository> userRepoMock = IMyPupilsReadOnlyRepositoryTestDoubles.MockFor(myPupils);

        Mock<IAggregatePupilsForMyPupilsApplicationService> mockAggregateService = AggregatePupilsForMyPupilsServiceTestDoubles.Default();

        Mock<IMapper<Pupil, MyPupilDto>> mockMapper = MapperTestDoubles.Default<Pupil, MyPupilDto>();

        GetMyPupilsRequest request = new(userId);

        // Act
        GetMyPupilsUseCase sut = new(
            userRepoMock.Object,
            mockAggregateService.Object,
            mockMapper.Object);

        GetMyPupilsResponse result = await sut.HandleRequestAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.MyPupils.Values);

        userRepoMock.Verify(repo =>
            repo.GetMyPupilsOrDefaultAsync(
                userId,
                It.IsAny<CancellationToken>()), Times.Once);

        mockAggregateService.Verify(
            t => t.GetPupilsAsync(It.Is<UniquePupilNumbers>(t => !t.GetUniquePupilNumbers().Any())), Times.Never);

        mockMapper.Verify(t => t.Map(It.IsAny<Pupil>()), Times.Never);
    }
}

