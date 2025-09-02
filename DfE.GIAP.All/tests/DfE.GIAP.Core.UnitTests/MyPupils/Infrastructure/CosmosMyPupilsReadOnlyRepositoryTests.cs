using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Query;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.Core.SharedTests.TestDoubles;
using DfE.GIAP.Core.UnitTests.TestDoubles;
using DfE.GIAP.Core.Users.Infrastructure.Repositories.Dtos;
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

        Func<CosmosDbMyPupilsReadOnlyRepository> construct = () => new CosmosDbMyPupilsReadOnlyRepository(
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
    public async Task GetUserByIdAsync_Throws_When_NonCosmosExceptionOccurs()
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
            repository.GetMyPupils(
                UserIdTestDoubles.Default(),
                It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task GetUserByIdAsync_LogsAndRethrows_When_CosmosExceptionIsThrown()
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
            repository.GetMyPupils(
                UserIdTestDoubles.Default(),
                It.IsAny<CancellationToken>()));

        string log = Assert.Single(mockLogger.Logs);
        Assert.Contains("CosmosException in GetMyPupils", log);
    }

}
