using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Command;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Domain;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.Write;
using DfE.GIAP.Core.UnitTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles.MyPupils;
using Microsoft.Azure.Cosmos;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Infrastructure;
public sealed class CosmosMyPupilsWriteOnlyRepositoryTests
{
    private const string MyPupilsContainerName = "mypupils";


    [Fact]
    public void Constructor_ThrowsArgumentNullException_When_LoggerIsNull()
    {
        // Arrange
        Func<CosmosDbMyPupilsWriteOnlyRepository> construct = () => new(
            logger: null!,
            cosmosDbCommandHandler: CosmosDbCommandHandlerTestDoubles.Default().Object,
            mapToDto: MapperTestDoubles.Default<MyPupilsAggregate, MyPupilsDocumentDto>().Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_When_CommandHandlerIsNull()
    {
        // Arrange
        Func<CosmosDbMyPupilsWriteOnlyRepository> construct = () => new(
            logger: LoggerTestDoubles.MockLogger<CosmosDbMyPupilsWriteOnlyRepository>(),
            cosmosDbCommandHandler: null!,
            mapToDto: MapperTestDoubles.Default<MyPupilsAggregate, MyPupilsDocumentDto>().Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_When_MapToDtoIsNull()
    {
        // Arrange
        Func<CosmosDbMyPupilsWriteOnlyRepository> construct = () => new(
            logger: LoggerTestDoubles.MockLogger<CosmosDbMyPupilsWriteOnlyRepository>(),
            cosmosDbCommandHandler: CosmosDbCommandHandlerTestDoubles.Default().Object,
            mapToDto: null!);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public async Task SaveMyPupilsAsync_Throws_When_NonCosmosExceptionOccurs()
    {
        // Arrange
        Mock<ICosmosDbCommandHandler> mockCosmosDbQueryHandler =
            CosmosDbCommandHandlerTestDoubles.MockForUpsertItemAsyncThrows<MyPupilsDocumentDto>(
                new Exception("test exception"));

        MyPupilsDocumentDto document = MyPupilsDocumentDtoTestDoubles.Default();

        Mock<IMapper<MyPupilsAggregate, MyPupilsDocumentDto>> mapperMock =
            MapperTestDoubles.MockFor<MyPupilsAggregate, MyPupilsDocumentDto>(stub: document);

        CosmosDbMyPupilsWriteOnlyRepository repository = new(
            logger: LoggerTestDoubles.MockLogger<CosmosDbMyPupilsWriteOnlyRepository>(),
            cosmosDbCommandHandler: mockCosmosDbQueryHandler.Object,
            mapToDto: mapperMock.Object);

        MyPupilsAggregate myPupils = MyPupilsAggregateTestDoubles.Default();

        // Act Assert
        await Assert.ThrowsAsync<Exception>(() => repository.SaveMyPupilsAsync(myPupils));
    }

    [Fact]
    public async Task SaveMyPupilsAsync_LogsAndRethrows_When_CosmosExceptionIsThrown()
    {
        // Arrange
        Mock<ICosmosDbCommandHandler> mockCosmosDbQueryHandler =
            CosmosDbCommandHandlerTestDoubles.MockForUpsertItemAsyncThrows<MyPupilsDocumentDto>(exception: CosmosExceptionTestDoubles.Default());

        InMemoryLogger<CosmosDbMyPupilsWriteOnlyRepository> inMemoryLogger = LoggerTestDoubles.MockLogger<CosmosDbMyPupilsWriteOnlyRepository>();

        MyPupilsDocumentDto document = MyPupilsDocumentDtoTestDoubles.Default();

        Mock<IMapper<MyPupilsAggregate, MyPupilsDocumentDto>> mapperMock =
            MapperTestDoubles.MockFor<MyPupilsAggregate, MyPupilsDocumentDto>(
                stub: document);

        CosmosDbMyPupilsWriteOnlyRepository repository = new(
            logger: inMemoryLogger,
            cosmosDbCommandHandler: mockCosmosDbQueryHandler.Object,
            mapToDto: mapperMock.Object);

        // Act Assert
        MyPupilsAggregate myPupils = MyPupilsAggregateTestDoubles.Default();

        await Assert.ThrowsAsync<CosmosException>(() => repository.SaveMyPupilsAsync(myPupils));

        string log = Assert.Single(inMemoryLogger.Logs);
        Assert.Contains($"SaveMyPupilsAsync Error in saving MyPupilsAsync for id: {myPupils.AggregateId.Value}", log);
    }

    [Fact]
    public async Task SaveMyPupilsAsync_WithEmptyUpns_UpsertsEmptyPupilList()
    {
        // Arrange
        UniquePupilNumbers uniquePupilNumbers = UniquePupilNumbers.Create(uniquePupilNumbers: []);

        MyPupilsAggregate myPupils = MyPupilsAggregateTestDoubles.Create(uniquePupilNumbers);

        MyPupilsDocumentDto document = MyPupilsDocumentDtoTestDoubles.Create(myPupils.AggregateId, uniquePupilNumbers);

        Mock<IMapper<MyPupilsAggregate, MyPupilsDocumentDto>> mapperMock =
            MapperTestDoubles.MockFor<MyPupilsAggregate, MyPupilsDocumentDto>(stub: document);

        Mock<ICosmosDbCommandHandler> commandHandlerDouble = CosmosDbCommandHandlerTestDoubles.Default();

        InMemoryLogger<CosmosDbMyPupilsWriteOnlyRepository> inMemoryLogger = LoggerTestDoubles.MockLogger<CosmosDbMyPupilsWriteOnlyRepository>();

        CosmosDbMyPupilsWriteOnlyRepository repository = new(
            logger: inMemoryLogger,
            cosmosDbCommandHandler: commandHandlerDouble.Object,
            mapToDto: mapperMock.Object);

        // Act
        await repository.SaveMyPupilsAsync(myPupils);

        // Assert

        commandHandlerDouble.Verify(handler =>
            handler.UpsertItemAsync(
                It.Is<MyPupilsDocumentDto>(
                    (dto) =>
                        dto.id == myPupils.AggregateId.Value &&
                            dto.MyPupils.Pupils != null &&
                                !dto.MyPupils.Pupils.Any()),
                MyPupilsContainerName,
                It.Is<string>((pk) => pk == myPupils.AggregateId.Value),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SaveMyPupilsAsync_MapsUpnsAndCallsUpsert_WithExpectedDto()
    {
        // Arrange
        UniquePupilNumbers uniquePupilNumbers =
            UniquePupilNumbers.Create(
                UniquePupilNumberTestDoubles.Generate(count: 3));

        MyPupilsAggregate myPupils = MyPupilsAggregateTestDoubles.Create(uniquePupilNumbers);

        MyPupilsDocumentDto document = MyPupilsDocumentDtoTestDoubles.Create(myPupils.AggregateId, uniquePupilNumbers);

        Mock<IMapper<MyPupilsAggregate, MyPupilsDocumentDto>> mapper =
            MapperTestDoubles.MockFor<MyPupilsAggregate, MyPupilsDocumentDto>(stub: document);

        Mock<ICosmosDbCommandHandler> commandHandlerDouble = CosmosDbCommandHandlerTestDoubles.Default();

        InMemoryLogger<CosmosDbMyPupilsWriteOnlyRepository> inMemoryLogger = LoggerTestDoubles.MockLogger<CosmosDbMyPupilsWriteOnlyRepository>();

        CosmosDbMyPupilsWriteOnlyRepository repository = new(inMemoryLogger, commandHandlerDouble.Object, mapper.Object);

        // Act
        await repository.SaveMyPupilsAsync(myPupils);

        // Assert
        commandHandlerDouble.Verify(handler =>
            handler.UpsertItemAsync(
                It.Is<MyPupilsDocumentDto>(
                    (dto) => dto.id == myPupils.AggregateId.Value &&
                        dto.MyPupils.Pupils.Select(pupil => pupil.UPN).SequenceEqual(uniquePupilNumbers.GetUniquePupilNumbers().Select(t => t.Value))),
                MyPupilsContainerName,
                It.Is<string>((pk) => pk == myPupils.AggregateId.Value),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
