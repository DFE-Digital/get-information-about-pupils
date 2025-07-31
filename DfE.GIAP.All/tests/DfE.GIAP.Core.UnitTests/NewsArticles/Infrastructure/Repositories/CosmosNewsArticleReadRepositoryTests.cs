﻿using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Query;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.NewsArticles.Application.Enums;
using DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories;
using DfE.GIAP.Core.SharedTests.TestDoubles;
using DfE.GIAP.Core.UnitTests.NewsArticles.UseCases;
using DfE.GIAP.Core.UnitTests.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.NewsArticles.Infrastructure.Repositories;

public sealed class CosmosNewsArticleReadRepositoryTests
{
    private readonly string _validId = "any_valid_id";
    private readonly InMemoryLogger<CosmosNewsArticleReadRepository> _mockLogger;

    public CosmosNewsArticleReadRepositoryTests()
    {
        _mockLogger = LoggerTestDoubles.MockLogger<CosmosNewsArticleReadRepository>();
    }

    [Fact]
    public void Constructor_ThrowsNullException_When_ReceivesNullLogger()
    {
        // Arrange
        Mock<IMapper<NewsArticleDto, NewsArticle>> mockMapper = MapperTestDoubles.Default<NewsArticleDto, NewsArticle>();
        Mock<ICosmosDbQueryHandler> mockQueryHandler = CosmosDbQueryHandlerTestDoubles.Default();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CosmosNewsArticleReadRepository(
            logger: null!,
            cosmosDbQueryHandler: mockQueryHandler.Object,
            dtoToEntityMapper: mockMapper.Object));
    }

    [Fact]
    public void Constructor_ThrowsNullException_When_ReceivesNullQueryHandler()
    {
        // Arrange
        Mock<IMapper<NewsArticleDto, NewsArticle>> mockMapper = MapperTestDoubles.Default<NewsArticleDto, NewsArticle>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CosmosNewsArticleReadRepository(
            logger: _mockLogger,
            cosmosDbQueryHandler: null!,
            dtoToEntityMapper: mockMapper.Object));
    }

    [Fact]
    public void Constructor_ThrowsNullException_When_ReceivesNullMapper()
    {
        // Arrange
        Mock<ICosmosDbQueryHandler> mockQueryHandler = CosmosDbQueryHandlerTestDoubles.Default();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CosmosNewsArticleReadRepository(
            logger: _mockLogger,
            cosmosDbQueryHandler: mockQueryHandler.Object,
            dtoToEntityMapper: null!));
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task GetNewsArticleByIdAsync_ThrowsNullException_When_Request_Id_IsNullOrEmpty_And_LogsWarning(string? id)
    {
        // Arrange
        Mock<ICosmosDbQueryHandler> mockQueryHandler = CosmosDbQueryHandlerTestDoubles.Default();
        Mock<IMapper<NewsArticleDto, NewsArticle>> mockMapper = MapperTestDoubles.Default<NewsArticleDto, NewsArticle>();

        CosmosNewsArticleReadRepository repository = new(
                    logger: _mockLogger,
                    cosmosDbQueryHandler: mockQueryHandler.Object,
                    dtoToEntityMapper: mockMapper.Object);

        Func<Task> act = () => repository.GetNewsArticleByIdAsync(id!);

        // Act Assert
        await Assert.ThrowsAsync<ArgumentException>(act);
        Assert.Equal("GetNewsArticleByIdAsync called with null or empty id.", _mockLogger.Logs.Single());
    }

    [Fact]
    public async Task GetNewsArticleByIdAsync_ReturnsNull_When_CosmosException()
    {
        // Arrange
        string requestId = _validId;

        Func<NewsArticleDto> cosmosExceptionGenerator = CosmosExceptionTestDoubles.ThrowsCosmosExceptionDelegate<NewsArticleDto>();

        Mock<ICosmosDbQueryHandler> mockQueryHandler =
            CosmosDbQueryHandlerTestDoubles.MockForReadById(cosmosExceptionGenerator);

        Mock<IMapper<NewsArticleDto, NewsArticle>> mockMapper = MapperTestDoubles.Default<NewsArticleDto, NewsArticle>();

        CosmosNewsArticleReadRepository sut = new(
                    logger: _mockLogger,
                    cosmosDbQueryHandler: mockQueryHandler.Object,
                    dtoToEntityMapper: mockMapper.Object);

        // Act
        NewsArticle? response = await sut.GetNewsArticleByIdAsync(requestId);

        //Assert
        Assert.Null(response);
        Assert.Equal($"CosmosException in GetNewsArticleByIdAsync for id: {requestId}", _mockLogger.Logs.Single());
        // TODO currently just verifying it was called, not what it was called with.
        mockQueryHandler.Verify(t => t.ReadItemByIdAsync<NewsArticleDto>(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            default), Times.Once());

        mockMapper.Verify(m => m.Map(It.IsAny<NewsArticleDto>()), Times.Never());
    }

    // TODO consider encapsulate the VerifyCalled into an extension around MockQueryHandler?

    [Fact]
    public async Task GetNewsArticleByIdAsync_BubblesException_When_MapperThrows()
    {
        // Arrange
        Mock<ICosmosDbQueryHandler> mockQueryHandler = CosmosDbQueryHandlerTestDoubles.Default();
        Mock<IMapper<NewsArticleDto, NewsArticle>> mockMapper = MapperTestDoubles.MockFor<NewsArticleDto, NewsArticle>(() => throw new Exception("Test exception"));

        CosmosNewsArticleReadRepository sut = new(
                    logger: _mockLogger,
                    cosmosDbQueryHandler: mockQueryHandler.Object,
                    dtoToEntityMapper: mockMapper.Object);

        // Act Assert
        Func<Task> act = () => sut.GetNewsArticleByIdAsync(_validId);
        await Assert.ThrowsAsync<Exception>(act);
    }

    [Fact]
    public async Task GetNewsArticleByIdAsync_ReturnsNull_When_MapperReturnsNull()
    {
        // Arrange
        Mock<IMapper<NewsArticleDto, NewsArticle>> mockMapper = MapperTestDoubles.MockFor<NewsArticleDto, NewsArticle>(stub: null);
        Mock<ICosmosDbQueryHandler> mockQueryHandler = CosmosDbQueryHandlerTestDoubles.Default();

        CosmosNewsArticleReadRepository sut = new(
                    logger: _mockLogger,
                    cosmosDbQueryHandler: mockQueryHandler.Object,
                    dtoToEntityMapper: mockMapper.Object);

        // Act
        NewsArticle? response = await sut.GetNewsArticleByIdAsync(_validId);

        // Assert
        Assert.Null(response);
        mockQueryHandler.Verify(t => t.ReadItemByIdAsync<NewsArticleDto>(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            default), Times.Once());
    }

    [Fact]
    public async Task GetNewsArticleByIdAsync_ReturnsNewsArticle_When_MapperReturnsNewsArticle()
    {
        // Arrange
        NewsArticle articleStub = NewsArticleTestDoubles.Create();
        Mock<IMapper<NewsArticleDto, NewsArticle>> mockMapper = MapperTestDoubles.MockFor<NewsArticleDto, NewsArticle>(articleStub);
        Mock<ICosmosDbQueryHandler> mockQueryHandler = CosmosDbQueryHandlerTestDoubles.Default();

        CosmosNewsArticleReadRepository sut = new(
                    logger: _mockLogger,
                    cosmosDbQueryHandler: mockQueryHandler.Object,
                    dtoToEntityMapper: mockMapper.Object);

        // Act
        NewsArticle? response = await sut.GetNewsArticleByIdAsync(_validId);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(articleStub, response);
        mockQueryHandler.Verify(t => t.ReadItemByIdAsync<NewsArticleDto>(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            default), Times.Once());
    }



    [Fact]
    public async Task GetNewsArticlesAsync_ReturnsEmptyArticlesList_When_CosmosException()
    {
        // Arrange
        Mock<IMapper<NewsArticleDto, NewsArticle>> mockMapper = MapperTestDoubles.Default<NewsArticleDto, NewsArticle>();

        Func<IEnumerable<NewsArticleDto>> cosmosExceptionGenerator =
            CosmosExceptionTestDoubles.ThrowsCosmosExceptionDelegate<IEnumerable<NewsArticleDto>>();

        Mock<ICosmosDbQueryHandler> mockQueryHandler = CosmosDbQueryHandlerTestDoubles.MockForReadMany(cosmosExceptionGenerator);

        CosmosNewsArticleReadRepository sut = new(
            logger: _mockLogger,
            cosmosDbQueryHandler: mockQueryHandler.Object,
            dtoToEntityMapper: mockMapper.Object);

        // Act
        IEnumerable<NewsArticle> response = await sut.GetNewsArticlesAsync(newsArticleSearchFilter: It.IsAny<NewsArticleSearchFilter>());

        // Assert
        Assert.Empty(response);
        Assert.Equal("CosmosException in GetNewsArticlesAsync.", _mockLogger.Logs.Single());
        mockMapper.Verify(
            (mapper) => mapper.Map(It.IsAny<NewsArticleDto>()), Times.Never());
    }

    [Fact]
    public async Task GetNewsArticlesAsync_ReturnsEmptyList_When_NoArticlesFound()
    {
        // Arrange
        Mock<IMapper<NewsArticleDto, NewsArticle>> mockMapper = MapperTestDoubles.Default<NewsArticleDto, NewsArticle>();
        Mock<ICosmosDbQueryHandler> mockQueryHandler = CosmosDbQueryHandlerTestDoubles.MockForReadMany<NewsArticleDto>(() => []);

        CosmosNewsArticleReadRepository sut = new(
                    logger: _mockLogger,
                    cosmosDbQueryHandler: mockQueryHandler.Object,
                    dtoToEntityMapper: mockMapper.Object);

        // Act
        IEnumerable<NewsArticle> response = await sut.GetNewsArticlesAsync(newsArticleSearchFilter: It.IsAny<NewsArticleSearchFilter>());

        // Assert
        Assert.Empty(response);
        mockQueryHandler.Verify(
            (handler) => handler.ReadItemsAsync<NewsArticleDto>(It.IsAny<string>(), It.IsAny<string>(), default),
                Times.Once());
    }

    [Theory]
    [InlineData(NewsArticleSearchFilter.Published, "c.Published=true")]
    [InlineData(NewsArticleSearchFilter.NotPublished, "c.Published=false")]
    [InlineData(NewsArticleSearchFilter.PublishedAndNotPublished, "(c.Published=true OR c.Published=false)")]
    public async Task GetNewsArticlesAsync_QueryConstructedCorrectly_When_Parameters_Passed_And_Handler_And_Mapper_Called(
        NewsArticleSearchFilter newsArticleSearchStatus, string expectedFilter)
    {
        // Arrange        
        const string ExpectedContainerName = "news";
        List<NewsArticleDto> newsArticleDTOs = NewsArticleDtoTestDoubles.Generate();

        Mock<ICosmosDbQueryHandler> mockQueryHandler = CosmosDbQueryHandlerTestDoubles.MockForReadMany(() => newsArticleDTOs);

        Mock<IMapper<NewsArticleDto, NewsArticle>> mockMapper = MapperTestDoubles.MockFor<NewsArticleDto, NewsArticle>(It.IsAny<NewsArticle>());

        CosmosNewsArticleReadRepository sut = new(
            logger: _mockLogger,
            cosmosDbQueryHandler: mockQueryHandler.Object,
            dtoToEntityMapper: mockMapper.Object);

        string expectedQuery = $"SELECT * FROM c WHERE {expectedFilter}";

        // Act
        // Force enumeration
        _ = (await sut.GetNewsArticlesAsync(newsArticleSearchStatus)).ToList();

        // Assert
        mockQueryHandler.Verify(
            (handler) => handler.ReadItemsAsync<NewsArticleDto>(ExpectedContainerName, expectedQuery, default), Times.Once());

        mockMapper.Verify(
            (mapper) => mapper.Map(It.IsAny<NewsArticleDto>()),
            Times.Exactly(newsArticleDTOs.Count));
    }
}
