using System.Net;
using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Command;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories;
using DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.Core.UnitTests.NewsArticles.Application.UseCases;
using DfE.GIAP.Core.UnitTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles;
using Microsoft.Azure.Cosmos;

namespace DfE.GIAP.Core.UnitTests.NewsArticles.Infrastructure.Repositories;

public sealed class CosmosDbNewsArticleWriteOnlyRepositoryTests
{
    private readonly InMemoryLoggerService _mockLogger;

    public CosmosDbNewsArticleWriteOnlyRepositoryTests()
    {
        _mockLogger = LoggerServiceTestDoubles.MockLoggerService();
    }

    [Fact]
    public void Constructor_ThrowsNullException_When_ReceivesNullLogger()
    {
        // Arrange
        Mock<IMapper<NewsArticle, NewsArticleDto>> mockMapper = MapperTestDoubles.Default<NewsArticle, NewsArticleDto>();
        Mock<ICosmosDbCommandHandler> mockCommandHandler = CosmosDbCommandHandlerTestDoubles.Default();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CosmosDbNewsArticleWriteOnlyRepository(
            logger: null!,
            cosmosDbCommandHandler: mockCommandHandler.Object,
            entityToDtoMapper: mockMapper.Object));
    }

    [Fact]
    public void Constructor_ThrowsNullException_When_ReceivesNullCommandHandler()
    {
        // Arrange
        Mock<IMapper<NewsArticle, NewsArticleDto>> mockMapper = MapperTestDoubles.Default<NewsArticle, NewsArticleDto>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CosmosDbNewsArticleWriteOnlyRepository(
            logger: _mockLogger,
            cosmosDbCommandHandler: null!,
            entityToDtoMapper: mockMapper.Object));
    }

    [Fact]
    public void Constructor_ThrowsNullException_When_ReceivesNullMapper()
    {
        // Arrange
        Mock<ICosmosDbCommandHandler> mockQueryHandler = CosmosDbCommandHandlerTestDoubles.Default();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CosmosDbNewsArticleWriteOnlyRepository(
            logger: _mockLogger,
            cosmosDbCommandHandler: mockQueryHandler.Object,
            entityToDtoMapper: null!));
    }

    [Fact]
    public async Task CreateNewsArticlesAsync_ThrowsArgumentNullException_When_NewsArticleIsNull()
    {
        // Arrange
        Mock<ICosmosDbCommandHandler> mockCommandHandler = CosmosDbCommandHandlerTestDoubles.Default();
        Mock<IMapper<NewsArticle, NewsArticleDto>> mockMapper = MapperTestDoubles.Default<NewsArticle, NewsArticleDto>();
        CosmosDbNewsArticleWriteOnlyRepository sut = new(
            logger: _mockLogger,
            cosmosDbCommandHandler: mockCommandHandler.Object,
            entityToDtoMapper: mockMapper.Object);

        // Act & Assert
        await Assert.ThrowsAnyAsync<ArgumentNullException>(() => sut.CreateNewsArticleAsync(null!));
    }

    [Theory]
    [InlineData(null)]
    public async Task CreateNewsArticleAsync_ThrowsArgumentException_When_TitleIsNull(string? title)
    {
        // Arrange
        Mock<ICosmosDbCommandHandler> mockCommandHandler = CosmosDbCommandHandlerTestDoubles.Default();
        Mock<IMapper<NewsArticle, NewsArticleDto>> mockMapper = MapperTestDoubles.Default<NewsArticle, NewsArticleDto>();
        CosmosDbNewsArticleWriteOnlyRepository sut = new(
            logger: _mockLogger,
            cosmosDbCommandHandler: mockCommandHandler.Object,
            entityToDtoMapper: mockMapper.Object);

        NewsArticle article = NewsArticleTestDoubles.Create() with { Title = title! };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.CreateNewsArticleAsync(article));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\n")]
    public async Task CreateNewsArticleAsync_ThrowsArgumentException_When_TitleIsEmpty(string title)
    {
        // Arrange
        Mock<ICosmosDbCommandHandler> mockCommandHandler = CosmosDbCommandHandlerTestDoubles.Default();
        Mock<IMapper<NewsArticle, NewsArticleDto>> mockMapper = MapperTestDoubles.Default<NewsArticle, NewsArticleDto>();
        CosmosDbNewsArticleWriteOnlyRepository sut = new(
            logger: _mockLogger,
            cosmosDbCommandHandler: mockCommandHandler.Object,
            entityToDtoMapper: mockMapper.Object);

        NewsArticle article = NewsArticleTestDoubles.Create() with { Title = title! };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => sut.CreateNewsArticleAsync(article));
    }

    [Theory]
    [InlineData(null)]
    public async Task CreateNewsArticleAsync_ThrowsArgumentException_When_BodyIsNull(string? body)
    {
        // Arrange
        Mock<ICosmosDbCommandHandler> mockCommandHandler = CosmosDbCommandHandlerTestDoubles.Default();
        Mock<IMapper<NewsArticle, NewsArticleDto>> mockMapper = MapperTestDoubles.Default<NewsArticle, NewsArticleDto>();
        CosmosDbNewsArticleWriteOnlyRepository sut = new(
            logger: _mockLogger,
            cosmosDbCommandHandler: mockCommandHandler.Object,
            entityToDtoMapper: mockMapper.Object);

        NewsArticle article = NewsArticleTestDoubles.Create() with { Body = body! };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.CreateNewsArticleAsync(article));
        mockMapper.Verify(m => m.Map(It.IsAny<NewsArticle>()), Times.Never());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\n")]
    public async Task CreateNewsArticleAsync_ThrowsArgumentException_When_BodyIsEmpty(string body)
    {
        // Arrange
        Mock<ICosmosDbCommandHandler> mockCommandHandler = CosmosDbCommandHandlerTestDoubles.Default();
        Mock<IMapper<NewsArticle, NewsArticleDto>> mockMapper = MapperTestDoubles.Default<NewsArticle, NewsArticleDto>();
        CosmosDbNewsArticleWriteOnlyRepository sut = new(
            logger: _mockLogger,
            cosmosDbCommandHandler: mockCommandHandler.Object,
            entityToDtoMapper: mockMapper.Object);

        NewsArticle article = NewsArticleTestDoubles.Create() with { Body = body! };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => sut.CreateNewsArticleAsync(article));
        mockMapper.Verify(m => m.Map(It.IsAny<NewsArticle>()), Times.Never());
    }

    [Fact]
    public async Task CreateNewsArticleAsync_BubblesException_When_MapperThrows()
    {
        // Arrange
        Mock<ICosmosDbCommandHandler> mockCommandHandler = CosmosDbCommandHandlerTestDoubles.Default();
        Mock<IMapper<NewsArticle, NewsArticleDto>> mockMapper = MapperTestDoubles.MockFor<NewsArticle, NewsArticleDto>(() => throw new Exception("Test exception"));

        CosmosDbNewsArticleWriteOnlyRepository sut = new(
            logger: _mockLogger,
            cosmosDbCommandHandler: mockCommandHandler.Object,
            entityToDtoMapper: mockMapper.Object);

        NewsArticle article = NewsArticleTestDoubles.Create();

        // Act
        Func<Task> act = () => sut.CreateNewsArticleAsync(article);

        // Assert
        await Assert.ThrowsAsync<Exception>(act);
        mockMapper.Verify(m => m.Map(It.IsAny<NewsArticle>()), Times.Once());
    }

    [Fact]
    public async Task CreateNewsArticleAsync_BubblesException_When_CosmosException()
    {
        // Arrange
        Mock<ICosmosDbCommandHandler> mockCommandHandler =
            CosmosDbCommandHandlerTestDoubles
            .MockForCreateNewsArticleAsyncThrows<NewsArticleDto>(
                new CosmosException("Simulated failure", HttpStatusCode.NotFound, 1, "activityId", 1.0));


        NewsArticleDto? articleDto = NewsArticleDtoTestDoubles.Generate(1).FirstOrDefault();
        Mock<IMapper<NewsArticle, NewsArticleDto>> mockMapper = MapperTestDoubles.MockFor<NewsArticle, NewsArticleDto>(articleDto);

        CosmosDbNewsArticleWriteOnlyRepository sut = new(
            logger: _mockLogger,
            cosmosDbCommandHandler: mockCommandHandler.Object,
            entityToDtoMapper: mockMapper.Object);

        // Act
        Func<Task> act = async () => await sut.CreateNewsArticleAsync(NewsArticleTestDoubles.Create());

        // Assert
        await Assert.ThrowsAsync<CosmosException>(act);
        Assert.Equal("CosmosException in CreateNewsArticleAsync", _mockLogger.Logs.Single());
        mockMapper.Verify(m => m.Map(It.IsAny<NewsArticle>()), Times.Once());
    }

    [Fact]
    public async Task CreateNewsArticleAsync_CallsMapperAndCommandHandler_When_ValidArticle()
    {
        // Arrange
        NewsArticleDto newsArticleDto = NewsArticleDtoTestDoubles.Generate(1).FirstOrDefault()!;
        Mock<IMapper<NewsArticle, NewsArticleDto>> mockMapper = MapperTestDoubles.MockFor<NewsArticle, NewsArticleDto>(newsArticleDto);

        Mock<ICosmosDbCommandHandler> mockCommandHandler = CosmosDbCommandHandlerTestDoubles.Default();

        CosmosDbNewsArticleWriteOnlyRepository sut = new(
            logger: _mockLogger,
            cosmosDbCommandHandler: mockCommandHandler.Object,
            entityToDtoMapper: mockMapper.Object);

        // Act
        await sut.CreateNewsArticleAsync(NewsArticleTestDoubles.Create());

        // Assert
        mockMapper.Verify(m => m.Map(It.IsAny<NewsArticle>()), Times.Once);
        mockCommandHandler.Verify(m => m.CreateItemAsync(It.IsAny<NewsArticleDto>(), It.IsAny<string>(), It.IsAny<string>(), default), Times.Once);
    }

    [Fact]
    public async Task DeleteNewsArticleAsync_CallsCommandHandler_When_ValidId()
    {
        // Arrange
        Mock<ICosmosDbCommandHandler> mockCommandHandler = CosmosDbCommandHandlerTestDoubles.Default();
        Mock<IMapper<NewsArticle, NewsArticleDto>> mockMapper = MapperTestDoubles.Default<NewsArticle, NewsArticleDto>();

        CosmosDbNewsArticleWriteOnlyRepository sut = new(
            logger: _mockLogger,
            cosmosDbCommandHandler: mockCommandHandler.Object,
            entityToDtoMapper: mockMapper.Object);

        NewsArticleIdentifier id = NewsArticleIdentifier.New();

        // Act
        await sut.DeleteNewsArticleAsync(id);

        // Assert
        mockCommandHandler.Verify(m => m.DeleteItemAsync<NewsArticleDto>(id.Value, It.IsAny<string>(), id.Value, default), Times.Once);
    }

    [Fact]
    public async Task DeleteNewsArticleAsync_BubblesException_When_CosmosException()
    {
        // Arrange
        Mock<ICosmosDbCommandHandler> mockCommandHandler =
            CosmosDbCommandHandlerTestDoubles
            .MockForDeleteItemAsyncThrows<NewsArticleDto>(
                new CosmosException("Simulated failure", HttpStatusCode.NotFound, 1, "activityId", 1.0));

        Mock<IMapper<NewsArticle, NewsArticleDto>> mockMapper = MapperTestDoubles.Default<NewsArticle, NewsArticleDto>();

        CosmosDbNewsArticleWriteOnlyRepository sut = new(
            logger: _mockLogger,
            cosmosDbCommandHandler: mockCommandHandler.Object,
            entityToDtoMapper: mockMapper.Object);

        // Act
        Func<Task> act = () => sut.DeleteNewsArticleAsync(NewsArticleIdentifier.New());

        // Assert
        await Assert.ThrowsAsync<CosmosException>(act);
        Assert.Equal("CosmosException in DeleteNewsArticleAsync", _mockLogger.Logs.Single());
        mockCommandHandler.Verify(m => m.DeleteItemAsync<NewsArticleDto>(
           It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default), Times.Once);
    }

    [Fact]
    public async Task UpdateNewsArticleAsync_ThrowsArgumentNullException_When_ArticleIsNull()
    {
        // Arrange
        Mock<ICosmosDbCommandHandler> mockCommandHandler = CosmosDbCommandHandlerTestDoubles.Default();
        Mock<IMapper<NewsArticle, NewsArticleDto>> mockMapper = MapperTestDoubles.Default<NewsArticle, NewsArticleDto>();

        CosmosDbNewsArticleWriteOnlyRepository sut = new(
            logger: _mockLogger,
            cosmosDbCommandHandler: mockCommandHandler.Object,
            entityToDtoMapper: mockMapper.Object);

        // Act & Assert
        await Assert.ThrowsAnyAsync<ArgumentNullException>(() => sut.UpdateNewsArticleAsync(null!));
    }

    [Fact]
    public async Task UpdateNewsArticleAsync_CallsMapperAndCommandHandler_When_ValidArticle()
    {
        // Arrange
        NewsArticleDto newsArticleDto = NewsArticleDtoTestDoubles.Generate(1).FirstOrDefault()!;
        Mock<IMapper<NewsArticle, NewsArticleDto>> mockMapper = MapperTestDoubles.MockFor<NewsArticle, NewsArticleDto>(newsArticleDto);
        Mock<ICosmosDbCommandHandler> mockCommandHandler = CosmosDbCommandHandlerTestDoubles.Default();

        CosmosDbNewsArticleWriteOnlyRepository sut = new(
            logger: _mockLogger,
            cosmosDbCommandHandler: mockCommandHandler.Object,
            entityToDtoMapper: mockMapper.Object);

        // Act
        await sut.UpdateNewsArticleAsync(NewsArticleTestDoubles.Create());

        // Assert
        mockMapper.Verify(m => m.Map(It.IsAny<NewsArticle>()), Times.Once);
        mockCommandHandler.Verify(m => m.ReplaceItemAsync(It.IsAny<NewsArticleDto>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default), Times.Once);
    }

    [Fact]
    public async Task UpdateNewsArticleAsync_BubblesException_When_CosmosException()
    {
        // Arrange
        Mock<ICosmosDbCommandHandler> mockCommandHandler =
            CosmosDbCommandHandlerTestDoubles
            .MockForReplaceItemAsyncThrows<NewsArticleDto>(
                new CosmosException("Simulated failure", HttpStatusCode.NotFound, 1, "activityId", 1.0));

        NewsArticleDto? articleDto = NewsArticleDtoTestDoubles.Generate(1).FirstOrDefault();
        Mock<IMapper<NewsArticle, NewsArticleDto>> mockMapper = MapperTestDoubles.MockFor<NewsArticle, NewsArticleDto>(articleDto);

        CosmosDbNewsArticleWriteOnlyRepository sut = new(
            logger: _mockLogger,
            cosmosDbCommandHandler: mockCommandHandler.Object,
            entityToDtoMapper: mockMapper.Object);

        // Act
        Func<Task> act = () => sut.UpdateNewsArticleAsync(NewsArticleTestDoubles.Create());

        // Assert
        await Assert.ThrowsAsync<CosmosException>(act);
        Assert.Equal("CosmosException in UpdateNewsArticleAsync", _mockLogger.Logs.Single());
        mockCommandHandler.Verify(m => m.ReplaceItemAsync(
            It.IsAny<NewsArticleDto>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default), Times.Once);
    }
}
