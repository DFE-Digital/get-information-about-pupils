using System.Net;
using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Query;
using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.Options;
using DfE.GIAP.Core.MyPupils.Domain;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.Read;
using DfE.GIAP.Core.UnitTests.TestDoubles.CosmosDb;
using DfE.GIAP.SharedTests.Common;
using DfE.GIAP.SharedTests.Features.MyPupils.DataTransferObjects;
using DfE.GIAP.SharedTests.Features.MyPupils.Domain;
using DfE.GIAP.SharedTests.Runtime.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles;
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
        Mock<IMapper<IEnumerable<string>, UniquePupilNumbers>> mapperMock = MapperTestDoubles.Default<IEnumerable<string>, UniquePupilNumbers>();


        Func<CosmosDbMyPupilsReadOnlyRepository> construct = () => new(
            logger: null!,
            cosmosDbQueryHandler: mockCosmosDbQueryHandler.Object,
            options,
            mapperMock.Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_When_QueryHandlerIsNull()
    {
        // Arrange
        InMemoryLogger<CosmosDbMyPupilsReadOnlyRepository> mockLogger = LoggerTestDoubles.Fake<CosmosDbMyPupilsReadOnlyRepository>();

        IOptions<MyPupilsOptions> options = OptionsTestDoubles.Default<MyPupilsOptions>();

        Mock<IMapper<IEnumerable<string>, UniquePupilNumbers>> mapperMock = MapperTestDoubles.Default<IEnumerable<string>, UniquePupilNumbers>();

        Func<CosmosDbMyPupilsReadOnlyRepository> construct = () => new(
            logger: mockLogger,
            cosmosDbQueryHandler: null!,
            options,
            mapperMock.Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_When_Options_IsNull()
    {
        // Arrange
        InMemoryLogger<CosmosDbMyPupilsReadOnlyRepository> mockLogger = LoggerTestDoubles.Fake<CosmosDbMyPupilsReadOnlyRepository>();

        Mock<ICosmosDbQueryHandler> mockCosmosDbQueryHandler = CosmosDbQueryHandlerTestDoubles.Default();

        Mock<IMapper<IEnumerable<string>, UniquePupilNumbers>> mapperMock = MapperTestDoubles.Default<IEnumerable<string>, UniquePupilNumbers>();

        Func<CosmosDbMyPupilsReadOnlyRepository> construct = () => new(
            logger: mockLogger,
            cosmosDbQueryHandler: mockCosmosDbQueryHandler.Object,
            myPupilsOptions: null!,
            mapperMock.Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_When_Mapper_IsNull()
    {
        // Arrange
        InMemoryLogger<CosmosDbMyPupilsReadOnlyRepository> mockLogger = LoggerTestDoubles.Fake<CosmosDbMyPupilsReadOnlyRepository>();

        Mock<ICosmosDbQueryHandler> mockCosmosDbQueryHandler = CosmosDbQueryHandlerTestDoubles.Default();

        IOptions<MyPupilsOptions> options = OptionsTestDoubles.Default<MyPupilsOptions>();

        Func<CosmosDbMyPupilsReadOnlyRepository> construct = () => new(
            logger: mockLogger,
            cosmosDbQueryHandler: mockCosmosDbQueryHandler.Object,
            myPupilsOptions: options,
            null!);

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

        InMemoryLogger<CosmosDbMyPupilsReadOnlyRepository> mockLogger = LoggerTestDoubles.Fake<CosmosDbMyPupilsReadOnlyRepository>();

        IOptions<MyPupilsOptions> options = OptionsTestDoubles.Default<MyPupilsOptions>();

        Mock<IMapper<IEnumerable<string>, UniquePupilNumbers>> mapperMock = MapperTestDoubles.Default<IEnumerable<string>, UniquePupilNumbers>();

        CosmosDbMyPupilsReadOnlyRepository repository = new(
            logger: mockLogger,
            cosmosDbQueryHandler: mockCosmosDbQueryHandler.Object,
            myPupilsOptions: options,
            mapToUniquePupilNumbers: mapperMock.Object);

        // Act & Assert

        await Assert.ThrowsAsync<Exception>(() =>
            repository.GetMyPupils(id: MyPupilsIdTestDoubles.Default()));
    }

    [Fact]
    public async Task GetMyPupilsOrDefaultAsync_LogsAndReturnsNull_When_CosmosException_IsThrown()
    {
        // Arrange
        Mock<ICosmosDbQueryHandler> mockCosmosDbQueryHandler =
            CosmosDbQueryHandlerTestDoubles.MockForTryReadById<MyPupilsDocumentDto>(
                () => throw CosmosExceptionTestDoubles.WithStatusCode(HttpStatusCode.InternalServerError));

        InMemoryLogger<CosmosDbMyPupilsReadOnlyRepository> mockLogger = LoggerTestDoubles.Fake<CosmosDbMyPupilsReadOnlyRepository>();

        IOptions<MyPupilsOptions> options = OptionsTestDoubles.Default<MyPupilsOptions>();

        Mock<IMapper<IEnumerable<string>, UniquePupilNumbers>> mapperMock = MapperTestDoubles.Default<IEnumerable<string>, UniquePupilNumbers>();

        CosmosDbMyPupilsReadOnlyRepository repository = new(
            logger: mockLogger,
            cosmosDbQueryHandler: mockCosmosDbQueryHandler.Object,
            myPupilsOptions: options,
            mapToUniquePupilNumbers: mapperMock.Object);

        // Act Assert
        MyPupilsAggregate? response = await repository.GetMyPupilsOrDefaultAsync(id: MyPupilsIdTestDoubles.Default());
        Assert.Null(response);

        string log = Assert.Single(mockLogger.Logs);
        Assert.Contains("CosmosException in GetMyPupilsOrDefaultAsync.", log);
    }

    [Fact]
    public async Task GetMyPupilsAsync_LogsAndReturnsNull_When_TryRead_Returns_Null()
    {
        // Arrange
        Mock<ICosmosDbQueryHandler> mockCosmosDbQueryHandler =
            CosmosDbQueryHandlerTestDoubles.MockForTryReadById<MyPupilsDto>(() => null);

        InMemoryLogger<CosmosDbMyPupilsReadOnlyRepository> mockLogger = LoggerTestDoubles.Fake<CosmosDbMyPupilsReadOnlyRepository>();

        IOptions<MyPupilsOptions> options = OptionsTestDoubles.Default<MyPupilsOptions>();

        Mock<IMapper<IEnumerable<string>, UniquePupilNumbers>> mapperMock = MapperTestDoubles.Default<IEnumerable<string>, UniquePupilNumbers>();

        CosmosDbMyPupilsReadOnlyRepository repository = new(
            logger: mockLogger,
            cosmosDbQueryHandler: mockCosmosDbQueryHandler.Object,
            myPupilsOptions: options,
            mapperMock.Object);

        MyPupilsId myPupilsId = MyPupilsIdTestDoubles.Default();

        // Act
        MyPupilsAggregate? myPupils = await repository.GetMyPupilsOrDefaultAsync(myPupilsId);

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

        MyPupilsAggregate myPupils = MyPupilsAggregateTestDoubles.Create(myPupilsId, upns);

        InMemoryLogger<CosmosDbMyPupilsReadOnlyRepository> mockLogger = LoggerTestDoubles.Fake<CosmosDbMyPupilsReadOnlyRepository>();

        MyPupilsDocumentDto myPupilsDocumentDto = MyPupilsDocumentDtoTestDoubles.Default();

        List<string> existingPupilsOnDocument = myPupilsDocumentDto.MyPupils.Pupils.Select(t => t.UPN).ToList();

        Mock<ICosmosDbQueryHandler> cosmosDbQueryHandlerMock =
            CosmosDbQueryHandlerTestDoubles.MockForTryReadById(() => myPupilsDocumentDto);

        Mock<IMapper<IEnumerable<string>, UniquePupilNumbers>> mapperMock =
            MapperTestDoubles.MockFor<IEnumerable<string>, UniquePupilNumbers>(stub: upns);

        IOptions<MyPupilsOptions> options = OptionsTestDoubles.Default<MyPupilsOptions>();

        CosmosDbMyPupilsReadOnlyRepository sut = new(
            logger: mockLogger,
            cosmosDbQueryHandler: cosmosDbQueryHandlerMock.Object,
            myPupilsOptions: options,
            mapToUniquePupilNumbers: mapperMock.Object);

        // Act
        MyPupilsAggregate? response = await sut.GetMyPupilsOrDefaultAsync(myPupilsId);

        // Assert
        Assert.NotNull(response);
        Assert.Equivalent(response, myPupils);

        cosmosDbQueryHandlerMock.Verify(
            (t) => t.TryReadItemByIdAsync<MyPupilsDocumentDto>(
                myPupilsId.Value,
                "mypupils",
                myPupilsId.Value,
                It.IsAny<CancellationToken>()), Times.Once);

        mapperMock.Verify(
            (mapper) => mapper.Map(
                It.Is<IEnumerable<string>>(
                    (ups) => ups.SequenceEqual(existingPupilsOnDocument))), Times.Once);
    }
}
