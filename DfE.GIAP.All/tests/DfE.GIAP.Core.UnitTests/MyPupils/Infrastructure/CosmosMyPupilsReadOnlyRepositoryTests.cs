using System.Net;
using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Query;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.Repositories;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.Read;
using DfE.GIAP.Core.SharedTests.TestDoubles;
using DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;
using DfE.GIAP.Core.UnitTests.TestDoubles;
using DfE.GIAP.Core.Users.Application;
using DfE.GIAP.SharedTests.TestDoubles;
using Microsoft.Azure.Cosmos;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Infrastructure;
public sealed class CosmosMyPupilsReadOnlyRepositoryTests
{
    [Fact]
    public void Constructor_ThrowsArgumentNullException_When_LoggerIsNull()
    {
        // Arrange
        Mock<ICosmosDbQueryHandler> mockCosmosDbQueryHandler = CosmosDbQueryHandlerTestDoubles.Default();

        Mock<IMapper<MyPupilsDocumentDto, Core.MyPupils.Application.Repositories.MyPupils>> mockMapper =
            MapperTestDoubles.Default<MyPupilsDocumentDto, Core.MyPupils.Application.Repositories.MyPupils>();

        Func<CosmosDbMyPupilsReadOnlyRepository> construct = () => new(
            logger: null!,
            cosmosDbQueryHandler: mockCosmosDbQueryHandler.Object,
            mockMapper.Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_When_QueryHandlerIsNull()
    {
        // Arrange
        InMemoryLogger<CosmosDbMyPupilsReadOnlyRepository> mockLogger = LoggerTestDoubles.MockLogger<CosmosDbMyPupilsReadOnlyRepository>();

        Mock<IMapper<MyPupilsDocumentDto, Core.MyPupils.Application.Repositories.MyPupils>> mockMapper =
            MapperTestDoubles.Default<MyPupilsDocumentDto, Core.MyPupils.Application.Repositories.MyPupils>();

        Func<CosmosDbMyPupilsReadOnlyRepository> construct = () => new(
            logger: mockLogger,
            cosmosDbQueryHandler: null!,
            mockMapper.Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_When_MapperIsNull()
    {
        // Arrange
        InMemoryLogger<CosmosDbMyPupilsReadOnlyRepository> mockLogger = LoggerTestDoubles.MockLogger<CosmosDbMyPupilsReadOnlyRepository>();

        Mock<ICosmosDbQueryHandler> mockCosmosDbQueryHandler = CosmosDbQueryHandlerTestDoubles.Default();

        Func<CosmosDbMyPupilsReadOnlyRepository> construct = () => new(
            logger: mockLogger,
            cosmosDbQueryHandler: mockCosmosDbQueryHandler.Object,
            mapper: null!);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public async Task GetMyPupilsAsync_Throws_When_NonCosmosExceptionOccurs()
    {
        // Arrange

        Mock<ICosmosDbQueryHandler> mockCosmosDbQueryHandler =
            CosmosDbQueryHandlerTestDoubles.MockForReadById<MyPupilsDocumentDto>(
                () => throw new Exception("test exception"));

        InMemoryLogger<CosmosDbMyPupilsReadOnlyRepository> mockLogger = LoggerTestDoubles.MockLogger<CosmosDbMyPupilsReadOnlyRepository>();

        Mock<IMapper<MyPupilsDocumentDto, Core.MyPupils.Application.Repositories.MyPupils>> mockMapper =
            MapperTestDoubles.Default<MyPupilsDocumentDto, Core.MyPupils.Application.Repositories.MyPupils>();

        CosmosDbMyPupilsReadOnlyRepository repository = new(
            logger: mockLogger,
            cosmosDbQueryHandler: mockCosmosDbQueryHandler.Object,
            mapper: mockMapper.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            repository.GetMyPupilsOrDefaultAsync(
                UserIdTestDoubles.Default(),
                It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task GetMyPupilsAsync_LogsAndRethrows_When_CosmosException_Non404_IsThrown()
    {
        // Arrange
        Mock<ICosmosDbQueryHandler> mockCosmosDbQueryHandler =
            CosmosDbQueryHandlerTestDoubles.MockForReadById<MyPupilsDocumentDto>(
                () => throw CosmosExceptionTestDoubles.Default());

        InMemoryLogger<CosmosDbMyPupilsReadOnlyRepository> mockLogger = LoggerTestDoubles.MockLogger<CosmosDbMyPupilsReadOnlyRepository>();

        Mock<IMapper<MyPupilsDocumentDto, Core.MyPupils.Application.Repositories.MyPupils>> mockMapper =
            MapperTestDoubles.Default<MyPupilsDocumentDto, Core.MyPupils.Application.Repositories.MyPupils>();

        CosmosDbMyPupilsReadOnlyRepository repository = new(
            logger: mockLogger,
            cosmosDbQueryHandler: mockCosmosDbQueryHandler.Object,
            mapper: mockMapper.Object);

        // Act Assert
        await Assert.ThrowsAsync<CosmosException>(() =>
            repository.GetMyPupilsOrDefaultAsync(
                UserIdTestDoubles.Default(),
                It.IsAny<CancellationToken>()));

        string log = Assert.Single(mockLogger.Logs);
        Assert.Contains("CosmosException in GetMyPupilsOrDefaultAsync", log);
    }

    [Fact]
    public async Task GetMyPupilsAsync_LogsAndReturnsNull_When_CosmosException_404_IsThrown()
    {
        // Arrange
        Mock<ICosmosDbQueryHandler> mockCosmosDbQueryHandler =
            CosmosDbQueryHandlerTestDoubles.MockForReadById<MyPupilsDocumentDto>(
                () => throw CosmosExceptionTestDoubles.WithStatusCode(HttpStatusCode.NotFound));

        InMemoryLogger<CosmosDbMyPupilsReadOnlyRepository> mockLogger = LoggerTestDoubles.MockLogger<CosmosDbMyPupilsReadOnlyRepository>();

        Mock<IMapper<MyPupilsDocumentDto, Core.MyPupils.Application.Repositories.MyPupils>> mockMapper =
            MapperTestDoubles.Default<MyPupilsDocumentDto, Core.MyPupils.Application.Repositories.MyPupils>();

        CosmosDbMyPupilsReadOnlyRepository repository = new(
            logger: mockLogger,
            cosmosDbQueryHandler: mockCosmosDbQueryHandler.Object,
            mapper: mockMapper.Object);

        // Act Assert

        UserId userId = UserIdTestDoubles.Default(); 
        Core.MyPupils.Application.Repositories.MyPupils? myPupils =
            await repository.GetMyPupilsOrDefaultAsync(userId, It.IsAny<CancellationToken>());

        Assert.Null(myPupils);

        string log = Assert.Single(mockLogger.Logs);
        Assert.Contains($"Could not find MyPupils for User id {userId.Value}", log);
    }

    [Fact]
    public async Task GetMyPupilsAsync_Returns_Mapped_MyPupils()
    {
        // Arrange
        UserId userId = UserIdTestDoubles.Default();

        Core.MyPupils.Application.Repositories.MyPupils myPupils = MyPupilsTestDoubles.Default();

        InMemoryLogger<CosmosDbMyPupilsReadOnlyRepository> mockLogger = LoggerTestDoubles.MockLogger<CosmosDbMyPupilsReadOnlyRepository>();

        Mock<IMapper<MyPupilsDocumentDto, Core.MyPupils.Application.Repositories.MyPupils>> mockMapper =
            MapperTestDoubles.MockFor<MyPupilsDocumentDto, Core.MyPupils.Application.Repositories.MyPupils>(
                stub: myPupils);

        MyPupilsDocumentDto myPupilsDocumentDto = MyPupilsDocumentDtoTestDoubles.Default();
        Mock<ICosmosDbQueryHandler> cosmosDbQueryHandlerMock =
            CosmosDbQueryHandlerTestDoubles.MockForReadById<MyPupilsDocumentDto>(() => myPupilsDocumentDto);

        CosmosDbMyPupilsReadOnlyRepository sut = new(
            logger: mockLogger,
            cosmosDbQueryHandler: cosmosDbQueryHandlerMock.Object,
            mapper: mockMapper.Object);

        // Act
        Core.MyPupils.Application.Repositories.MyPupils? response = await sut.GetMyPupilsOrDefaultAsync(userId, It.IsAny<CancellationToken>());

        // Assert

        Assert.NotNull(response);
        Assert.Equivalent(response, myPupils);
        mockMapper.Verify(t => t.Map(myPupilsDocumentDto), Times.Once);
        cosmosDbQueryHandlerMock.Verify(
            (t) => t.ReadItemByIdAsync<MyPupilsDocumentDto>(
                userId.Value,
                "mypupils",
                userId.Value,
                It.IsAny<CancellationToken>()), Times.Once);
    }

}
