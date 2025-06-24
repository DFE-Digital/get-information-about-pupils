using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Command;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.NewsArticles.Application.Models;
using DfE.GIAP.Core.NewsArticles.Application.Repositories;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories;

/// <summary>
/// Provides functionality for creating and managing news articles in a Cosmos DB container.
/// </summary>
/// <remarks>This repository is responsible for handling write operations for news articles in the Cosmos DB. It
/// uses a command handler to interact with the database and a mapper to convert between domain entities and data
/// transfer objects.</remarks>
internal class CosmosNewsArticleWriteRepository : INewsArticleWriteRepository
{
    private const string ContainerName = "news";
    private readonly ILogger<CosmosNewsArticleWriteRepository> _logger;
    private readonly ICosmosDbCommandHandler _cosmosDbCommandHandler;
    private readonly IMapper<NewsArticle, NewsArticleDTO> _entityToDtoMapper;

    public CosmosNewsArticleWriteRepository(
        ILogger<CosmosNewsArticleWriteRepository> logger,
        ICosmosDbCommandHandler cosmosDbCommandHandler,
        IMapper<NewsArticle, NewsArticleDTO> entityToDtoMapper)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(cosmosDbCommandHandler);
        ArgumentNullException.ThrowIfNull(entityToDtoMapper);
        _logger = logger;
        _cosmosDbCommandHandler = cosmosDbCommandHandler;
        _entityToDtoMapper = entityToDtoMapper;
    }

    /// <summary>
    /// Asynchronously creates a new news article in the database.
    /// </summary>
    /// <remarks>This method maps the provided <see cref="NewsArticle"/> object to a data transfer object
    /// (DTO) and stores it in the database. If the operation fails due to a database error, a <see
    /// cref="CosmosException"/> is thrown.</remarks>
    /// <param name="newsArticle">The <see cref="NewsArticle"/> object representing the news article to be created. The <see
    /// cref="NewsArticle.Title"/> and <see cref="NewsArticle.Body"/> properties must not be null or whitespace.</param>
    /// <returns></returns>
    public async Task CreateNewsArticleAsync(NewsArticle newsArticle)
    {
        ArgumentNullException.ThrowIfNull(newsArticle);
        ArgumentException.ThrowIfNullOrWhiteSpace(newsArticle.Title);
        ArgumentException.ThrowIfNullOrWhiteSpace(newsArticle.Body);

        try
        {
            NewsArticleDTO newsArticleDto = _entityToDtoMapper.Map(newsArticle);
            await _cosmosDbCommandHandler.CreateItemAsync(newsArticleDto, ContainerName, newsArticleDto.Id);
        }
        catch (CosmosException ex)
        {
            _logger.LogCritical(ex, "CosmosException in CreateNewsArticleAsync.");
            throw;
        }
    }

    /// <summary>
    /// Deletes a news article from the database asynchronously.
    /// </summary>
    /// <remarks> If the operation fails, a <see cref="CosmosException"/> is logged and rethrown.</remarks>
    /// <param name="id">The identifier of the news article to delete. Cannot be <see langword="null"/>.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    public async Task DeleteNewsArticleAsync(NewsArticleIdentifier id)
    {
        try
        {
            await _cosmosDbCommandHandler.DeleteItemAsync<NewsArticleDTO>(id.Value, ContainerName, id.Value);
        }
        catch (CosmosException ex)
        {
            _logger.LogCritical(ex, "CosmosException in DeleteNewsArticleAsync.");
            throw;
        }
    }

    /// <summary>
    /// Updates an existing news article in the database.
    /// </summary>
    /// <remarks>This method maps the provided <see cref="NewsArticle"/> object to a data transfer object
    /// (DTO) and updates it in the database. If the operation fails due to a database error, a <see
    /// cref="CosmosException"/> is thrown.</remarks>
    /// <param name="newsArticle">The news article to update. Cannot be <see langword="null"/>.</param>
    /// <returns></returns>
    public async Task UpdateNewsArticleAsync(NewsArticle newsArticle)
    {
        ArgumentNullException.ThrowIfNull(newsArticle);

        try
        {
            NewsArticleDTO newsArticleDto = _entityToDtoMapper.Map(newsArticle);
            await _cosmosDbCommandHandler.ReplaceItemAsync(newsArticleDto, newsArticleDto.Id, ContainerName, newsArticleDto.Id);
        }
        catch (CosmosException ex)
        {
            _logger.LogCritical(ex, "CosmosException in UpdateNewsArticleAsync.");
            throw;
        }
    }
}
