using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.Repositories;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Core.MyPupils.Domain;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;
using DfE.GIAP.SharedTests.Common;
using DfE.GIAP.SharedTests.Features.MyPupils.Application;
using DfE.GIAP.SharedTests.Features.MyPupils.Domain;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Application.UseCases.GetMyPupils;
public sealed class GetMyPupilsUseCaseTests
{
    [Fact]
    public async Task HandleRequestAsync_ReturnsMappedPupils()
    {
        // Arrange
        MyPupilsId myPupilsId = MyPupilsIdTestDoubles.Default();

        MyPupilsAggregate myPupils = MyPupilsAggregateTestDoubles.Default();

        Mock<IMyPupilsReadOnlyRepository> readRepositoryMock = IMyPupilsReadOnlyRepositoryTestDoubles.MockForGetMyPupilsOrDefault(myPupils);

        List<Pupil> pupils =
            myPupils.GetMyPupils()
                .Select((upn) => PupilBuilder.CreateBuilder(upn).Build())
                .ToList();

        Mock<IAggregatePupilsForMyPupilsApplicationService> aggregateServiceMock = AggregatePupilsForMyPupilsServiceTestDoubles.MockFor(pupils);

        Mock<IMapper<Pupil, MyPupilsModel>> mapperMock = MapperTestDoubles.Default<Pupil, MyPupilsModel>();
        MyPupilsModels myPupilDtos = MyPupilsModelTestDoubles.GenerateWithUniquePupilNumbers(pupils.Select(t => t.Identifier));

        mapperMock.MockMappingForMany(pupils, myPupilDtos.Values.ToList());

        GetMyPupilsRequest request = new(myPupilsId.Value);

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
            repo.GetMyPupilsOrDefaultAsync(myPupilsId), Times.Once);

        aggregateServiceMock.Verify(
            t => t.GetPupilsAsync(
                It.Is<UniquePupilNumbers>(
                    (upns) => upns.GetUniquePupilNumbers().SequenceEqual(myPupils.GetMyPupils()))),
            Times.Once);

        mapperMock.Verify(
            t => t.Map(It.IsAny<Pupil>()), Times.Exactly(pupils.Count));
    }

    [Fact]
    public async Task HandleRequestAsync_WithNullRepositoryResponse_ReturnsEmptyResponse()
    {
        // Arrange
        MyPupilsId myPupilsId = MyPupilsIdTestDoubles.Default();

        Mock<IMyPupilsReadOnlyRepository> userRepoMock = IMyPupilsReadOnlyRepositoryTestDoubles.MockForGetMyPupilsOrDefault(null!);

        Mock<IAggregatePupilsForMyPupilsApplicationService> mockAggregateService = AggregatePupilsForMyPupilsServiceTestDoubles.Default();

        Mock<IMapper<Pupil, MyPupilsModel>> mockMapper = MapperTestDoubles.Default<Pupil, MyPupilsModel>();

        GetMyPupilsRequest request = new(myPupilsId.Value);

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
            repo.GetMyPupilsOrDefaultAsync(myPupilsId), Times.Once);

        mockAggregateService.Verify(
            t => t.GetPupilsAsync(It.Is<UniquePupilNumbers>(t => !t.GetUniquePupilNumbers().Any())), Times.Never);

        mockMapper.Verify(t => t.Map(It.IsAny<Pupil>()), Times.Never);
    }

    [Fact]
    public async Task HandleRequestAsync_WithEmptyPupilList_ReturnsEmptyResponse()
    {
        // Arrange
        MyPupilsId myPupilsId = MyPupilsIdTestDoubles.Default();

        MyPupilsAggregate myPupils =

            MyPupilsAggregateTestDoubles.Create(
                UniquePupilNumbers.Empty());

        Mock<IMyPupilsReadOnlyRepository> userRepoMock = IMyPupilsReadOnlyRepositoryTestDoubles.MockForGetMyPupilsOrDefault(myPupils);

        Mock<IAggregatePupilsForMyPupilsApplicationService> mockAggregateService = AggregatePupilsForMyPupilsServiceTestDoubles.Default();

        Mock<IMapper<Pupil, MyPupilsModel>> mockMapper = MapperTestDoubles.Default<Pupil, MyPupilsModel>();

        GetMyPupilsRequest request = new(myPupilsId.Value);

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
                myPupilsId), Times.Once);

        mockAggregateService.Verify(
            t => t.GetPupilsAsync(
                It.Is<UniquePupilNumbers>(t => !t.GetUniquePupilNumbers().Any())), Times.Never);

        mockMapper.Verify(t => t.Map(It.IsAny<Pupil>()), Times.Never);
    }
}
