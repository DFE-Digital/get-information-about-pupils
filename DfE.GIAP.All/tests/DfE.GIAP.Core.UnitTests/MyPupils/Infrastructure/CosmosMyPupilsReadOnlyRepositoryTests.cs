using System.Net;
using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Query;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.Options;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.Read;
using DfE.GIAP.Core.UnitTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles.MyPupils;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Infrastructure;
public sealed class CosmosMyPupilsReadOnlyRepositoryTests
{
    [Fact]
    public void Constructor_ThrowsArgumentNullException_When_LoggerIsNull()
    {
        // Arrange
        Mock<ICosmosDbQueryHandler> mockCosmosDbQueryHandler = CosmosDbQueryHandlerTestDoubles.Default();
        IOptions<MyPupilsOptions> options = OptionsTestDoubles.Default<MyPupilsOptions>();


        Func<CosmosDbMyPupilsReadOnlyRepository> construct = () => new(
            logger: null!,
            cosmosDbQueryHandler: mockCosmosDbQueryHandler.Object,
            options);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_When_QueryHandlerIsNull()
    {
        // Arrange
        InMemoryLogger<CosmosDbMyPupilsReadOnlyRepository> mockLogger = LoggerTestDoubles.MockLogger<CosmosDbMyPupilsReadOnlyRepository>();

        IOptions<MyPupilsOptions> options = OptionsTestDoubles.Default<MyPupilsOptions>();

        Func<CosmosDbMyPupilsReadOnlyRepository> construct = () => new(
            logger: mockLogger,
            cosmosDbQueryHandler: null!,
            options);

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
            myPupilsOptions: null!);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public async Task GetMyPupilsAsync_Throws_When_NonCosmosExceptionOccurs()
    {
        // Arrange

        Mock<ICosmosDbQueryHandler> mockCosmosDbQueryHandler =
            CosmosDbQueryHandlerTestDoubles.MockForTryReadById<MyPupilsDocumentDto>(
                () => throw new Exception("test exception"));

        InMemoryLogger<CosmosDbMyPupilsReadOnlyRepository> mockLogger = LoggerTestDoubles.MockLogger<CosmosDbMyPupilsReadOnlyRepository>();

        IOptions<MyPupilsOptions> options = OptionsTestDoubles.Default<MyPupilsOptions>();

        CosmosDbMyPupilsReadOnlyRepository repository = new(
            logger: mockLogger,
            cosmosDbQueryHandler: mockCosmosDbQueryHandler.Object,
            myPupilsOptions: options);

        // Act & Assert

        await Assert.ThrowsAsync<Exception>(() =>
            repository.GetMyPupilsOrDefaultAsync(id: MyPupilsIdTestDoubles.Default()));
    }

    [Fact]
    public async Task GetMyPupilsAsync_LogsAndRethrows_When_CosmosException_Non404_IsThrown()
    {
        // Arrange
        Mock<ICosmosDbQueryHandler> mockCosmosDbQueryHandler =
            CosmosDbQueryHandlerTestDoubles.MockForTryReadById<MyPupilsDocumentDto>(
                () => throw CosmosExceptionTestDoubles.WithStatusCode(HttpStatusCode.InternalServerError));

        InMemoryLogger<CosmosDbMyPupilsReadOnlyRepository> mockLogger = LoggerTestDoubles.MockLogger<CosmosDbMyPupilsReadOnlyRepository>();

        IOptions<MyPupilsOptions> options = OptionsTestDoubles.Default<MyPupilsOptions>();

        CosmosDbMyPupilsReadOnlyRepository repository = new(
            logger: mockLogger,
            cosmosDbQueryHandler: mockCosmosDbQueryHandler.Object,
            myPupilsOptions: options);

        // Act Assert
        await Assert.ThrowsAsync<CosmosException>(() =>
            repository.GetMyPupilsOrDefaultAsync(id: MyPupilsIdTestDoubles.Default()));

        string log = Assert.Single(mockLogger.Logs);
        Assert.Contains("CosmosException in GetMyPupilsOrDefaultAsync", log);
    }

    [Fact]
    public async Task GetMyPupilsAsync_LogsAndReturnsNull_When_TryRead_Returns_Null()
    {
        // Arrange
        Mock<ICosmosDbQueryHandler> mockCosmosDbQueryHandler =
            CosmosDbQueryHandlerTestDoubles.MockForTryReadById<MyPupilsDocumentDto>(() => null);

        InMemoryLogger<CosmosDbMyPupilsReadOnlyRepository> mockLogger = LoggerTestDoubles.MockLogger<CosmosDbMyPupilsReadOnlyRepository>();

        IOptions<MyPupilsOptions> options = OptionsTestDoubles.Default<MyPupilsOptions>();

        CosmosDbMyPupilsReadOnlyRepository repository = new(
            logger: mockLogger,
            cosmosDbQueryHandler: mockCosmosDbQueryHandler.Object,
            myPupilsOptions: options);

        MyPupilsId myPupilsId = MyPupilsIdTestDoubles.Default();

        // Act
        Core.MyPupils.Domain.AggregateRoot.MyPupils? myPupils = await repository.GetMyPupilsOrDefaultAsync(myPupilsId);

        // Assert
        Assert.Null(myPupils);

        string log = Assert.Single(mockLogger.Logs);
        Assert.Contains($"Could not find MyPupils for User id {myPupilsId.Value}", log);
    }

    [Fact]
    public async Task GetMyPupilsAsync_Returns_Mapped_MyPupils()
    {
        // Arrange
        MyPupilsId myPupilsId = MyPupilsIdTestDoubles.Default();

        UniquePupilNumbers upns =
            UniquePupilNumbers.Create(
                UniquePupilNumberTestDoubles.Generate(count: 10));

        Core.MyPupils.Domain.AggregateRoot.MyPupils myPupils = MyPupilsAggregateRootTestDoubles.Create(myPupilsId, upns);

        InMemoryLogger<CosmosDbMyPupilsReadOnlyRepository> mockLogger = LoggerTestDoubles.MockLogger<CosmosDbMyPupilsReadOnlyRepository>();

        Mock<IMapper<MyPupilsDocumentDto, Core.MyPupils.Domain.AggregateRoot.MyPupils>> mockMapper =
            MapperTestDoubles.MockFor<MyPupilsDocumentDto, Core.MyPupils.Domain.AggregateRoot.MyPupils>(stub: myPupils);

        MyPupilsDocumentDto myPupilsDocumentDto = MyPupilsDocumentDtoTestDoubles.Default();
        
        Mock<ICosmosDbQueryHandler> cosmosDbQueryHandlerMock =
            CosmosDbQueryHandlerTestDoubles.MockForTryReadById(() => myPupilsDocumentDto);

        IOptions<MyPupilsOptions> options = OptionsTestDoubles.Default<MyPupilsOptions>();

        CosmosDbMyPupilsReadOnlyRepository sut = new(
            logger: mockLogger,
            cosmosDbQueryHandler: cosmosDbQueryHandlerMock.Object,
            myPupilsOptions: options);

        // Act
        Core.MyPupils.Domain.AggregateRoot.MyPupils? response = await sut.GetMyPupilsOrDefaultAsync(myPupilsId);

        // Assert

        Assert.NotNull(response);
        Assert.Equivalent(response, myPupils);
        
        cosmosDbQueryHandlerMock.Verify(
            (t) => t.ReadItemByIdAsync<MyPupilsDocumentDto>(
                myPupilsId.Value,
                "mypupils",
                myPupilsId.Value,
                It.IsAny<CancellationToken>()), Times.Once);

        mockMapper.Verify(t => t.Map(myPupilsDocumentDto), Times.Once);

    }

}
