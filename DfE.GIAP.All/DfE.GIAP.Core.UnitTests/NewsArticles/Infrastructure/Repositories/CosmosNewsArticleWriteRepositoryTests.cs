// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Command;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories;
using DfE.GIAP.Core.SharedTests.TestDoubles;
using DfE.GIAP.Core.UnitTests.NewsArticles.UseCases;
using DfE.GIAP.Core.UnitTests.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.NewsArticles.Infrastructure.Repositories;
public sealed class CosmosNewsArticleWriteRepositoryTests
{
    private readonly InMemoryLogger<CosmosNewsArticleWriteRepository> _mockLogger;

    public CosmosNewsArticleWriteRepositoryTests()
    {
        _mockLogger = LoggerTestDoubles.MockLogger<CosmosNewsArticleWriteRepository>();
    }

    [Fact]
    public void Constructor_ThrowsNullException_When_ReceivesNullLogger()
    {
        // Arrange
        Mock<IMapper<NewsArticle, NewsArticleDTO>> mockMapper = MapperTestDoubles.DefaultFromTo<NewsArticle, NewsArticleDTO>();
        Mock<ICosmosDbCommandHandler> mockCommandHandler = CosmosDbCommandHandlerTestDoubles.Default();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CosmosNewsArticleWriteRepository(
            logger: null!,
            cosmosDbCommandHandler: mockCommandHandler.Object,
            entityToDtoMapper: mockMapper.Object));
    }

    [Fact]
    public void Constructor_ThrowsNullException_When_ReceivesNullCommandHandler()
    {
        // Arrange
        Mock<IMapper<NewsArticle, NewsArticleDTO>> mockMapper = MapperTestDoubles.DefaultFromTo<NewsArticle, NewsArticleDTO>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CosmosNewsArticleWriteRepository(
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
        Assert.Throws<ArgumentNullException>(() => new CosmosNewsArticleWriteRepository(
            logger: _mockLogger,
            cosmosDbCommandHandler: mockQueryHandler.Object,
            entityToDtoMapper: null!));
    }

    [Fact]
    public async Task CreateNewsArticlesAsync_ThrowsArgumentNullException_When_NewsArticleIsNull()
    {
        // Arrange
        Mock<ICosmosDbCommandHandler> mockCommandHandler = CosmosDbCommandHandlerTestDoubles.Default();
        Mock<IMapper<NewsArticle, NewsArticleDTO>> mockMapper = MapperTestDoubles.DefaultFromTo<NewsArticle, NewsArticleDTO>();
        CosmosNewsArticleWriteRepository sut = new(
            logger: _mockLogger,
            cosmosDbCommandHandler: mockCommandHandler.Object,
            entityToDtoMapper: mockMapper.Object);

        // Act & Assert
        await Assert.ThrowsAnyAsync<ArgumentNullException>(() => sut.CreateNewsArticleAsync(null!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\n")]
    public async Task CreateNewsArticleAsync_ThrowsArgumentException_When_TitleIsNullOrEmpty(string? title)
    {
        // Arrange
        Mock<ICosmosDbCommandHandler> mockCommandHandler = CosmosDbCommandHandlerTestDoubles.Default();
        Mock<IMapper<NewsArticle, NewsArticleDTO>> mockMapper = MapperTestDoubles.DefaultFromTo<NewsArticle, NewsArticleDTO>();
        CosmosNewsArticleWriteRepository sut = new(
            logger: _mockLogger,
            cosmosDbCommandHandler: mockCommandHandler.Object,
            entityToDtoMapper: mockMapper.Object);

        NewsArticle article = NewsArticleTestDoubles.Create() with { Title = title! };

        // Act & Assert
        ArgumentException ex = await Assert.ThrowsAsync<ArgumentException>(() => sut.CreateNewsArticleAsync(article));
        Assert.Equal("Title cannot be null or empty. (Parameter 'Title')", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\n")]
    public async Task CreateNewsArticleAsync_ThrowsArgumentException_When_BodyIsNullOrEmpty(string? body)
    {
        // Arrange
        Mock<ICosmosDbCommandHandler> mockCommandHandler = CosmosDbCommandHandlerTestDoubles.Default();
        Mock<IMapper<NewsArticle, NewsArticleDTO>> mockMapper = MapperTestDoubles.DefaultFromTo<NewsArticle, NewsArticleDTO>();
        CosmosNewsArticleWriteRepository sut = new(
            logger: _mockLogger,
            cosmosDbCommandHandler: mockCommandHandler.Object,
            entityToDtoMapper: mockMapper.Object);

        NewsArticle article = NewsArticleTestDoubles.Create() with { Body = body! };

        // Act & Assert
        ArgumentException ex = await Assert.ThrowsAsync<ArgumentException>(() => sut.CreateNewsArticleAsync(article));
        Assert.Equal("Body cannot be null or empty. (Parameter 'Body')", ex.Message);
        mockMapper.Verify(m => m.Map(It.IsAny<NewsArticle>()), Times.Never());
    }

    [Fact]
    public async Task CreateNewsArticleAsync_BubblesException_When_MapperThrows()
    {
        // Arrange
        Mock<ICosmosDbCommandHandler> mockCommandHandler = CosmosDbCommandHandlerTestDoubles.Default();
        Mock<IMapper<NewsArticle, NewsArticleDTO>> mockMapper = MapperTestDoubles.MockMapperFromTo<NewsArticle, NewsArticleDTO>(() => throw new Exception("Test exception"));

        CosmosNewsArticleWriteRepository sut = new(
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
        Func<NewsArticleDTO> cosmosExceptionGenerator =
            CosmosExceptionTestDoubles.ThrowsCosmosExceptionDelegate<NewsArticleDTO>();
        Mock<ICosmosDbCommandHandler> mockCommandHandler = CosmosDbCommandHandlerTestDoubles.MockForCreateItemAsync(cosmosExceptionGenerator);

        NewsArticleDTO? articleDto = NewsArticleDTOTestDoubles.Generate(1).FirstOrDefault();
        Mock<IMapper<NewsArticle, NewsArticleDTO>> mockMapper = MapperTestDoubles.MockMapperFromTo<NewsArticle, NewsArticleDTO>(articleDto);

        CosmosNewsArticleWriteRepository sut = new(
            logger: _mockLogger,
            cosmosDbCommandHandler: mockCommandHandler.Object,
            entityToDtoMapper: mockMapper.Object);

        // Act
        await sut.CreateNewsArticleAsync(NewsArticleTestDoubles.Create());

        // Assert
        Assert.Equal("CosmosException in CreateNewsArticleAsync.", _mockLogger.Logs.Single());
        mockMapper.Verify(m => m.Map(It.IsAny<NewsArticle>()), Times.Once());
    }

    [Fact]
    public async Task CreateNewsArticleAsync_CallsMapperAndCommandHandler_When_ValidArticle()
    {
        // Arrange
        NewsArticleDTO newsArticleDto = NewsArticleDTOTestDoubles.Generate(1).FirstOrDefault()!;
        Mock<IMapper<NewsArticle, NewsArticleDTO>> mockMapper = MapperTestDoubles.MockMapperFromTo<NewsArticle, NewsArticleDTO>(newsArticleDto);

        Mock<ICosmosDbCommandHandler> mockCommandHandler = CosmosDbCommandHandlerTestDoubles.Default();

        CosmosNewsArticleWriteRepository sut = new(
            logger: _mockLogger,
            cosmosDbCommandHandler: mockCommandHandler.Object,
            entityToDtoMapper: mockMapper.Object);

        // Act
        await sut.CreateNewsArticleAsync(NewsArticleTestDoubles.Create());

        // Assert
        mockMapper.Verify(m => m.Map(It.IsAny<NewsArticle>()), Times.Once);
        mockCommandHandler.Verify(m => m.CreateItemAsync(It.IsAny<NewsArticleDTO>(), It.IsAny<string>(), It.IsAny<string>(), default), Times.Once);
    }
}
