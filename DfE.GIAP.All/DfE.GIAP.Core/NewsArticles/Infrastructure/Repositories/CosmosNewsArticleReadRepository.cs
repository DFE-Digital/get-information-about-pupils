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
/// Implements <see cref="INewsArticleReadRepository"/> for querying news data.
/// </summary>
internal class CosmosNewsArticleReadRepository : INewsArticleReadRepository
{
    private const string ContainerName = "news";
    private readonly ILogger<CosmosNewsArticleReadRepository> _logger;
    private readonly ICosmosDbQueryHandler _cosmosDbQueryHandler;
    private readonly IMapper<NewsArticleDTO, NewsArticle> _dtoToEntityMapper;

    public CosmosNewsArticleReadRepository(
        ILogger<CosmosNewsArticleReadRepository> logger,
        ICosmosDbQueryHandler cosmosDbQueryHandler,
        IMapper<NewsArticleDTO, NewsArticle> dtoToEntityMapper)
    {
        _logger = logger ??
            throw new ArgumentNullException(nameof(logger));
        _cosmosDbQueryHandler = cosmosDbQueryHandler ??
            throw new ArgumentNullException(nameof(cosmosDbQueryHandler));
        _dtoToEntityMapper = dtoToEntityMapper ??
            throw new ArgumentNullException(nameof(dtoToEntityMapper));
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
            _logger.LogWarning("GetNewsArticleByIdAsync called with null or empty id.");
            throw new ArgumentException("Id must not be null or empty.", nameof(id));
        }

        try
        {
            NewsArticleDTO queryResponse = await _cosmosDbQueryHandler.ReadItemByIdAsync<NewsArticleDTO>(id, ContainerName, id);

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
    /// Retrieves a collection of news articles based on the specified search filter.
    /// </summary>
    /// <remarks>This method queries a Cosmos DB container to retrieve news articles based on the provided
    /// filter. The filter determines whether articles are archived, published, or both. In the event of a <see
    /// cref="CosmosException"/>, the method logs the error and returns an empty collection.</remarks>
    /// <param name="newsArticleSearchFilter">A filter that specifies the criteria for retrieving news articles, such as whether they are archived, published,
    /// or both.</param>
    /// <returns>An asynchronous task that returns an <see cref="IEnumerable{T}"/> of <see cref="NewsArticle"/> objects matching
    /// the specified filter. If no articles match the filter, an empty collection is returned.</returns>
    public async Task<IEnumerable<NewsArticle>> GetNewsArticlesAsync(NewsArticleSearchFilter newsArticleSearchFilter)
    {
        try
        {
            string filter = newsArticleSearchFilter.ToCosmosFilters();
            string query = $"SELECT * FROM c WHERE {filter}";

            IEnumerable<NewsArticleDTO> queryResponse = await _cosmosDbQueryHandler
                .ReadItemsAsync<NewsArticleDTO>(ContainerName, query);

            IEnumerable<NewsArticle> mappedResponse = queryResponse.Select(_dtoToEntityMapper.Map);
            return mappedResponse;
        }
        catch (CosmosException ex)
        {
            _logger.LogCritical(ex, "CosmosException in GetNewsArticlesAsync.");
            return Enumerable.Empty<NewsArticle>();
        }
    }
}
