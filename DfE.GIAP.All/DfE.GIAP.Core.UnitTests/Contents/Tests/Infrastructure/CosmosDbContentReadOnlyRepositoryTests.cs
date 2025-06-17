using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Query;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Contents.Application.Models;
using DfE.GIAP.Core.Contents.Infrastructure.Repositories;
using DfE.GIAP.Core.SharedTests.TestDoubles;
using DfE.GIAP.Core.UnitTests.TestDoubles;
using Microsoft.Azure.Cosmos;

namespace DfE.GIAP.Core.UnitTests.Contents.Tests.Infrastructure;
public sealed class CosmosDbContentReadOnlyRepositoryTests
{
    private readonly InMemoryLogger<CosmosDbContentReadOnlyRepository> _mockLogger;
    private readonly Mock<IMapper<ContentDto?, Content>> _mockMapper;
    private readonly Mock<ICosmosDbQueryHandler> _mockCosmosDbQueryHandler;
    public CosmosDbContentReadOnlyRepositoryTests()
    {
        _mockLogger = LoggerTestDoubles.MockLogger<CosmosDbContentReadOnlyRepository>();
        _mockMapper = MapperTestDoubles.DefaultFromTo<ContentDto?, Content>();
        _mockCosmosDbQueryHandler = CosmosDbQueryHandlerTestDoubles.Default();
    }

    [Fact]
    public void CosmosDbContentReadOnlyRepository_Constructor_ThrowsNullException_When_CreatedWithNull_Logger()
    {
        // Arrange
        Action construct = () => new CosmosDbContentReadOnlyRepository(
            logger: null!,
            contentDtoToContentMapper: _mockMapper.Object,
            cosmosDbQueryHandler: _mockCosmosDbQueryHandler.Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void CosmosDbContentReadOnlyRepository_Constructor_ThrowsNullException_When_CreatedWithNull_Mapper()
    {
        // Arrange
        Action construct = () => new CosmosDbContentReadOnlyRepository(
            logger: _mockLogger,
            contentDtoToContentMapper: null!,
            cosmosDbQueryHandler: _mockCosmosDbQueryHandler.Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void CosmosDbContentReadOnlyRepository_Constructor_ThrowsNullException_When_CreatedWithNull_CosmosQueryHandler()
    {
        // Arrange
        Action construct = () => new CosmosDbContentReadOnlyRepository(
            logger: _mockLogger,
            contentDtoToContentMapper: _mockMapper.Object,
            cosmosDbQueryHandler: null!);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public async Task CosmosDbContentReadOnlyRepository_GetContentByIdAsync_Throws_When_NonCosmosExceptionOccurs()
    {
        // Arrange
        _mockCosmosDbQueryHandler
            .Setup(q => q.ReadItemsAsync<ContentDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(() => throw new Exception("test exception"));

        CosmosDbContentReadOnlyRepository repository = new(
            logger: _mockLogger,
            contentDtoToContentMapper: _mockMapper.Object,
            cosmosDbQueryHandler: _mockCosmosDbQueryHandler.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => repository.GetContentByIdAsync(ContentKey.Create("stub-key")));
    }

    [Fact]
    public async Task CosmosDbContentReadOnlyRepository_GetContentByIdAsync_LogsAndRethrows_When_CosmosExceptionIsThrown()
    {
        // Arrange
        string validPageKey = "valid-key";

        CosmosException cosmosException = CosmosExceptionTestDoubles.Default();

        _mockCosmosDbQueryHandler
            .Setup(q => q.ReadItemsAsync<ContentDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(cosmosException);

        CosmosDbContentReadOnlyRepository repository = new(
            logger: _mockLogger,
            contentDtoToContentMapper: _mockMapper.Object,
            cosmosDbQueryHandler: _mockCosmosDbQueryHandler.Object);

        // Act Assert
        await Assert.ThrowsAsync<CosmosException>(() => repository.GetContentByIdAsync(ContentKey.Create(validPageKey)));
        string log = Assert.Single(_mockLogger.Logs);
        Assert.Contains("CosmosException in GetContentByIdAsync", log);
    }


    [Fact]
    public async Task CosmosDbContentReadOnlyRepository_GetContentByIdAsync_Returns_MappedContent()
    {
        // Arrange
        string expectedContainerName = "application-data";
        string validPageKey = "valid-key";
        string expectedQuery = $"SELECT * FROM c WHERE c.DOCTYPE = 20 AND c.id = '{validPageKey}'";

        ContentDto contentDto = ContentDtoTestDoubles.Generate(1).Single();

        Content expectedOutputContent = new()
        {
            Title = contentDto.Title,
            Body = contentDto.Body
        };

        _mockCosmosDbQueryHandler
            .Setup((queryHandler) => queryHandler.ReadItemsAsync<ContentDto>(expectedContainerName, expectedQuery, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { contentDto });

        _mockMapper
            .Setup(m => m.Map(contentDto))
            .Returns(expectedOutputContent);

        CosmosDbContentReadOnlyRepository repository = new(
            logger: _mockLogger,
            contentDtoToContentMapper: _mockMapper.Object,
            cosmosDbQueryHandler: _mockCosmosDbQueryHandler.Object);

        // Act
        Content result = await repository.GetContentByIdAsync(ContentKey.Create(validPageKey));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedOutputContent, result);

        _mockCosmosDbQueryHandler
            .Verify((queryHandler) => queryHandler.ReadItemsAsync<ContentDto>(expectedContainerName, expectedQuery, It.IsAny<CancellationToken>()),
            Times.Once);

        _mockMapper.Verify(m => m.Map(contentDto), Times.Once);
    }
}
