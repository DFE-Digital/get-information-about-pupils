using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Query;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.NewsArticles.Application.Enums;
using DfE.GIAP.Core.NewsArticles.Application.Extensions;
using DfE.GIAP.Core.NewsArticles.Application.Models;
using DfE.GIAP.Core.NewsArticles.Application.Repositories;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories;

/// <summary>
/// Repository for reading news articles from Azure Cosmos DB.
/// Implements <see cref="INewsArticleReadOnlyRepository"/> for querying news data.
/// </summary>
internal class CosmosNewsArticleReadOnlyRepository : INewsArticleReadOnlyRepository
{
    private const string ContainerName = "news";
    private readonly ILogger<CosmosNewsArticleReadOnlyRepository> _logger;
    private readonly ICosmosDbQueryHandler _cosmosDbQueryHandler;
    private readonly IMapper<NewsArticleDto, NewsArticle> _dtoToEntityMapper;

    public CosmosNewsArticleReadOnlyRepository(
        ILogger<CosmosNewsArticleReadOnlyRepository> logger,
        ICosmosDbQueryHandler cosmosDbQueryHandler,
        IMapper<NewsArticleDto, NewsArticle> dtoToEntityMapper)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(cosmosDbQueryHandler);
        ArgumentNullException.ThrowIfNull(dtoToEntityMapper);
        _logger = logger;
        _cosmosDbQueryHandler = cosmosDbQueryHandler;
        _dtoToEntityMapper = dtoToEntityMapper;
    }


    /// <summary>
    /// Asynchronously retrieves a news article from Cosmos DB by its unique identifier.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the news article to retrieve. Must not be <see langword="null"/> or empty.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The result contains the <see cref="NewsArticle"/> if found;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the <paramref name="id"/> is <see langword="null"/> or an empty string.
    /// </exception>
    /// <remarks>
    /// Logs a warning if the input is invalid and logs a critical error if a Cosmos DB exception occurs during retrieval.
    /// </remarks>

    public async Task<NewsArticle?> GetNewsArticleByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            _logger.LogCritical("GetNewsArticleByIdAsync called with null or empty id.");
            throw new ArgumentException("Id must not be null or empty.", nameof(id));
        }

        try
        {
            NewsArticleDto queryResponse = await _cosmosDbQueryHandler.ReadItemByIdAsync<NewsArticleDto>(id, ContainerName, id);

            NewsArticle mappedResponse = _dtoToEntityMapper.Map(queryResponse);
            return mappedResponse;
        }
        catch (CosmosException ex)
        {
            _logger.LogCritical(ex, "CosmosException in GetNewsArticleByIdAsync for id: {Id}", id);
            return null;
        }
    }


    /// <summary>
    /// Retrieves a collection of news articles that match the specified search filter.
    /// </summary>
    /// <remarks>This method queries a Cosmos DB container using the provided filter criteria and maps the results
    /// to a collection of <see cref="NewsArticle"/> objects. Ensure that the <paramref name="newsArticleSearchFilter"/> 
    /// is properly configured to avoid invalid queries.</remarks>
    /// <param name="newsArticleSearchFilter">The filter criteria used to search for news articles. This includes parameters such as keywords,  date ranges, or
    /// other attributes to refine the search.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection  of <see
    /// cref="NewsArticle"/> objects that match the search criteria. If no articles are found, the  collection will be
    /// empty.</returns>
    public async Task<IEnumerable<NewsArticle>> GetNewsArticlesAsync(NewsArticleSearchFilter newsArticleSearchFilter)
    {
        try
        {
            string filter = newsArticleSearchFilter.ToCosmosFilters();
            string query = $"SELECT * FROM c WHERE {filter}";

            IEnumerable<NewsArticleDto> queryResponse = await _cosmosDbQueryHandler
                .ReadItemsAsync<NewsArticleDto>(ContainerName, query);

            IEnumerable<NewsArticle> mappedResponse = queryResponse.Select(_dtoToEntityMapper.Map);
            return mappedResponse;
        }
        catch (CosmosException ex)
        {
            _logger.LogCritical(ex, "CosmosException in GetNewsArticlesAsync.");
            return [];
        }
    }



    public async Task<bool> HasArticlesBeenModifiedSinceAsync(DateTime expectedTime)
    {
        try
        {
            string isoDate = expectedTime.ToString("o");
            string query = $"SELECT TOP 1 * FROM c WHERE c.lastModifiedTime > '{isoDate}' AND c.isPublished = true";

            IEnumerable<NewsArticleDto> results = await _cosmosDbQueryHandler.ReadItemsAsync<NewsArticleDto>(ContainerName, query);
            return results.Any();
        }
        catch (CosmosException ex)
        {
            _logger.LogCritical(ex, "CosmosException in HasArticlesModifiedSinceAsync.");
            return false;
        }
    }
}
