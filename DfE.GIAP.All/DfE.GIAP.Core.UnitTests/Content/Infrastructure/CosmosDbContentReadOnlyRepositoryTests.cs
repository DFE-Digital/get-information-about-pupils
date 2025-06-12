using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Query;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Content.Application.Model;
using DfE.GIAP.Core.Content.Application.Options;
using DfE.GIAP.Core.Content.Application.Options.Provider;
using DfE.GIAP.Core.Content.Infrastructure.Repositories;
using DfE.GIAP.Core.SharedTests.TestDoubles;
using DfE.GIAP.Core.UnitTests.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Content.Infrastructure;
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
    public void ContentReadOnlyRepository_Constructor_ThrowsNullException_When_CreatedWithNull_Logger()
    {
        Action construct = () => new CosmosDbContentReadOnlyRepository(
            logger: null!,
            contentDtoToContentMapper: _mockMapper.Object,
            pageContentOptionProvider: _mockPageContentOptionsProvider.Object,
            cosmosDbQueryHandler: _mockCosmosDbQueryHandler.Object);
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void ContentReadOnlyRepository_Constructor_ThrowsNullException_When_CreatedWithNull_Mapper()
    {
        Action construct = () => new CosmosDbContentReadOnlyRepository(
            logger: _mockLogger,
            contentDtoToContentMapper: null!,
            pageContentOptionProvider: _mockPageContentOptionsProvider.Object,
            cosmosDbQueryHandler: _mockCosmosDbQueryHandler.Object);
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void ContentReadOnlyRepository_Constructor_ThrowsNullException_When_CreatedWithNull_PageContentOptionsProvider()
    {
        Action construct = () => new CosmosDbContentReadOnlyRepository(
            logger: _mockLogger,
            contentDtoToContentMapper: _mockMapper.Object,
            pageContentOptionProvider: null!,
            cosmosDbQueryHandler: _mockCosmosDbQueryHandler.Object);
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void ContentReadOnlyRepository_Constructor_ThrowsNullException_When_CreatedWithNull_CosmosQueryHandler()
    {
        Action construct = () => new CosmosDbContentReadOnlyRepository(
            logger: _mockLogger,
            contentDtoToContentMapper: _mockMapper.Object,
            pageContentOptionProvider: _mockPageContentOptionsProvider.Object,
            cosmosDbQueryHandler: null!);
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public async Task ContentReadOnlyRepository_GetContentByKeyAsync_Throws_When_OptionsProvider_Returns_NullOrEmptyDocumentId(string? documentId)
    {
        PageContentOption toValidatePageContentOption = new()
        {
            DocumentId = documentId
        };

        _mockPageContentOptionsProvider.Setup(
            (t) => t.GetPageContentOptionWithPageKey(It.IsAny<string>()))
                    .Returns(toValidatePageContentOption);

        CosmosDbContentReadOnlyRepository repository = new CosmosDbContentReadOnlyRepository(
            logger: _mockLogger,
            contentDtoToContentMapper: _mockMapper.Object,
            pageContentOptionProvider: _mockPageContentOptionsProvider.Object,
            cosmosDbQueryHandler: _mockCosmosDbQueryHandler.Object);

        ContentKey stubKey = ContentKey.Create("valid-key");
        await Assert.ThrowsAnyAsync<ArgumentException>(() => repository.GetContentByKeyAsync(stubKey));
    }
}

internal static class PageContentOptionsProviderTestDoubles
{
    internal static Mock<IPageContentOptionsProvider> Default() => new();
}
