using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Query;
using DfE.GIAP.Core.Common.CrossCutting;
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
    /// Asynchronously retrieves a collection of news articles from Cosmos DB based on their archived and draft status.
    /// </summary>
    /// <param name="isArchived">
    /// Indicates whether to include archived articles. Set to <see langword="true"/> to include archived articles.
    /// </param>
    /// <param name="isDraft">
    /// Indicates whether to include draft articles. Set to <see langword="true"/> to include draft articles.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The result contains an <see cref="IEnumerable{T}"/> of 
    /// <see cref="NewsArticle"/> objects that match the specified criteria. Returns an empty collection if an error occurs.
    /// </returns>
    /// <remarks>
    /// Constructs a SQL query dynamically based on the provided filters and executes it against the Cosmos DB container.
    /// Logs critical errors if a Cosmos DB exception is encountered.
    /// </remarks>

    public async Task<IEnumerable<NewsArticle>> GetNewsArticlesAsync(bool isArchived, bool? isDraft)
    {
        try
        {
            string publishedFilter = isDraft switch
            {
                true => "c.Published=false",
                false => "c.Published=true",
                null => "(c.Published=true OR c.Published=false)"
            };
            string archivedFilter = isArchived ? "c.Archived=true" : "c.Archived=false";
            string query = $"SELECT * FROM c WHERE {archivedFilter} And {publishedFilter}";

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
