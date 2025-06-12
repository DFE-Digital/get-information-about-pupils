// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Command;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories;
using DfE.GIAP.Core.SharedTests.TestDoubles;
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
}
