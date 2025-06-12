using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Query;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Content.Application.Model;
using DfE.GIAP.Core.Content.Application.Options;
using DfE.GIAP.Core.Content.Application.Options.Provider;
using DfE.GIAP.Core.Content.Infrastructure.Repositories;
using DfE.GIAP.Core.SharedTests.TestDoubles;
using DfE.GIAP.Core.UnitTests.Content.TestDoubles;
using DfE.GIAP.Core.UnitTests.TestDoubles;
using Microsoft.Azure.Cosmos;

namespace DfE.GIAP.Core.UnitTests.Content.Tests.Infrastructure;
public sealed class CosmosDbContentReadOnlyRepositoryTests
{
    private readonly InMemoryLogger<CosmosDbContentReadOnlyRepository> _mockLogger;
    private readonly Mock<IMapper<ContentDTO?, Core.Content.Application.Model.Content>> _mockMapper;
    private readonly Mock<IPageContentOptionsProvider> _mockPageContentOptionsProvider;
    private readonly Mock<ICosmosDbQueryHandler> _mockCosmosDbQueryHandler;
    public CosmosDbContentReadOnlyRepositoryTests()
    {
        _mockLogger = LoggerTestDoubles.MockLogger<CosmosDbContentReadOnlyRepository>();
        _mockMapper = MapperTestDoubles.DefaultFromTo<ContentDTO?, Core.Content.Application.Model.Content>();
        _mockPageContentOptionsProvider = PageContentOptionsProviderTestDoubles.Default();
        _mockCosmosDbQueryHandler = CosmosDbQueryHandlerTestDoubles.Default();
    }

    [Fact]
    public void CosmosDbContentReadOnlyRepository_Constructor_ThrowsNullException_When_CreatedWithNull_Logger()
    {
        // Arrange
        Action construct = () => new CosmosDbContentReadOnlyRepository(
            logger: null!,
            contentDtoToContentMapper: _mockMapper.Object,
            pageContentOptionProvider: _mockPageContentOptionsProvider.Object,
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
            pageContentOptionProvider: _mockPageContentOptionsProvider.Object,
            cosmosDbQueryHandler: _mockCosmosDbQueryHandler.Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void CosmosDbContentReadOnlyRepository_Constructor_ThrowsNullException_When_CreatedWithNull_PageContentOptionsProvider()
    {
        // Arrange
        Action construct = () => new CosmosDbContentReadOnlyRepository(
            logger: _mockLogger,
            contentDtoToContentMapper: _mockMapper.Object,
            pageContentOptionProvider: null!,
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
            pageContentOptionProvider: _mockPageContentOptionsProvider.Object,
            cosmosDbQueryHandler: null!);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public async Task CosmosDbContentReadOnlyRepository_GetContentByKeyAsync_Throws_When_OptionsProvider_Returns_NullOrEmptyDocumentId(string? invalidDocumentId)
    {
        // Arrange
        PageContentOption pageContentOption = PageContentOptionTestDoubles.StubFor(invalidDocumentId);

        _mockPageContentOptionsProvider
            .Setup((t) => t.GetPageContentOptionWithPageKey(It.IsAny<string>()))
            .Returns(pageContentOption);

        CosmosDbContentReadOnlyRepository repository = new(
            logger: _mockLogger,
            contentDtoToContentMapper: _mockMapper.Object,
            pageContentOptionProvider: _mockPageContentOptionsProvider.Object,
            cosmosDbQueryHandler: _mockCosmosDbQueryHandler.Object);

        Func<Task<Core.Content.Application.Model.Content>> act = () => repository.GetContentByKeyAsync(ContentKey.Create("stub-key"));

        // Act Assert
        await Assert.ThrowsAnyAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task CosmosDbContentReadOnlyRepository_GetContentByKeyAsync_Throws_When_NonCosmosExceptionOccurs()
    {
        // Arrange
        _mockPageContentOptionsProvider
            .Setup(p => p.GetPageContentOptionWithPageKey(It.IsAny<string>()))
            .Returns(() => throw new Exception("Test exception"));

        CosmosDbContentReadOnlyRepository repository = new(
            logger: _mockLogger,
            contentDtoToContentMapper: _mockMapper.Object,
            pageContentOptionProvider: _mockPageContentOptionsProvider.Object,
            cosmosDbQueryHandler: _mockCosmosDbQueryHandler.Object);

        // Act & Assert
        ContentKey contentKey = ContentKey.Create("any-key");
        await Assert.ThrowsAsync<Exception>(() => repository.GetContentByKeyAsync(contentKey));
    }

    [Fact]
    public async Task CosmosDbContentReadOnlyRepository_GetContentByKeyAsync_LogsAndRethrows_When_CosmosExceptionIsThrown()
    {
        // Arrange
        string validPageKey = "valid-key";
        string validDocumentId = "valid-documentid";

        PageContentOption pageContentOption = PageContentOptionTestDoubles.StubFor(validDocumentId);

        CosmosException cosmosException = CosmosExceptionTestDoubles.Default();

        _mockPageContentOptionsProvider
            .Setup(p => p.GetPageContentOptionWithPageKey(validPageKey))
            .Returns(pageContentOption);

        _mockCosmosDbQueryHandler
            .Setup(q => q.ReadItemsAsync<ContentDTO>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(cosmosException);

        CosmosDbContentReadOnlyRepository repository = new(
            logger: _mockLogger,
            contentDtoToContentMapper: _mockMapper.Object,
            pageContentOptionProvider: _mockPageContentOptionsProvider.Object,
            cosmosDbQueryHandler: _mockCosmosDbQueryHandler.Object);

        ContentKey contentKey = ContentKey.Create(validPageKey);

        // Act Assert
        await Assert.ThrowsAsync<CosmosException>(() => repository.GetContentByKeyAsync(contentKey));
        string log = Assert.Single(_mockLogger.Logs);
        Assert.Contains("CosmosException in GetContentByKeyAsync", log);
    }


    [Fact]
    public async Task CosmosDbContentReadOnlyRepository_GetContentByKeyAsync_Returns_MappedContent()
    {
        // Arrange
        string expectedContainerName = "application-data";
        string validPageKey = "valid-key";
        string validDocumentId = "valid-documentid";
        string expectedQuery = $"SELECT * FROM c WHERE c.DOCTYPE = 20 AND c.id = '{validDocumentId}'";

        ContentKey contentKey = ContentKey.Create(validPageKey);
        PageContentOption pageContentOption = PageContentOptionTestDoubles.StubFor(validDocumentId);

        ContentDTO contentDto = ContentDTOTestDoubles.Generate(1).Single();

        Core.Content.Application.Model.Content expectedOutputContent = new()
        {
            Title = contentDto.Title,
            Body = contentDto.Body
        };

        _mockPageContentOptionsProvider
            .Setup(provider => provider.GetPageContentOptionWithPageKey(validPageKey))
            .Returns(pageContentOption);

        _mockCosmosDbQueryHandler
            .Setup((queryHandler) => queryHandler.ReadItemsAsync<ContentDTO>(expectedContainerName, expectedQuery, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { contentDto });

        _mockMapper
            .Setup(m => m.Map(contentDto))
            .Returns(expectedOutputContent);

        CosmosDbContentReadOnlyRepository repository = new(
            logger: _mockLogger,
            contentDtoToContentMapper: _mockMapper.Object,
            pageContentOptionProvider: _mockPageContentOptionsProvider.Object,
            cosmosDbQueryHandler: _mockCosmosDbQueryHandler.Object);

        // Act
        Core.Content.Application.Model.Content result = await repository.GetContentByKeyAsync(contentKey);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedOutputContent, result);

        _mockPageContentOptionsProvider.Verify(
            (provider) => provider.GetPageContentOptionWithPageKey(validPageKey),
            Times.Once);

        _mockCosmosDbQueryHandler.Verify(
            queryHandler => queryHandler.ReadItemsAsync<ContentDTO>(expectedContainerName, expectedQuery, It.IsAny<CancellationToken>()),
            Times.Once);

        _mockMapper.Verify(m => m.Map(contentDto), Times.Once);
    }
}
