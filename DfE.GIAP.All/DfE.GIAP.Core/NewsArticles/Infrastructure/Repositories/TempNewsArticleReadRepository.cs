using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.NewsArticles.Application.Models;
using DfE.GIAP.Core.NewsArticles.Application.Repositories;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Container = Microsoft.Azure.Cosmos.Container;

namespace DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories;

internal class TempNewsArticleReadRepository : INewsArticleReadRepository
{
    private const string ContainerName = "application-data";
    private const string DatabaseId = "giapsearch";
    private readonly ILogger<TempNewsArticleReadRepository> _logger;
    private readonly CosmosClient _cosmosClient;
    private readonly IMapper<NewsArticleDTO, NewsArticle> _dtoToEntityMapper;

    public TempNewsArticleReadRepository(
        ILogger<TempNewsArticleReadRepository> logger,
        CosmosClient cosmosClient,
        IMapper<NewsArticleDTO, NewsArticle> dtoToEntityMapper)
    {
        _logger = logger ??
            throw new ArgumentNullException(nameof(logger));
        _cosmosClient = cosmosClient ??
            throw new ArgumentNullException(nameof(cosmosClient));
        _dtoToEntityMapper = dtoToEntityMapper ??
            throw new ArgumentNullException(nameof(dtoToEntityMapper));
    }

    public async Task<NewsArticle?> GetNewsArticleByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            _logger.LogWarning("GetNewsArticleByIdAsync called with null or empty id.");
            throw new ArgumentException("Id must not be null or empty.", nameof(id));
        }

        try
        {
            string query = $"SELECT * FROM c WHERE c.DOCTYPE=7 AND c.id='{id}'";
            Container container = _cosmosClient.GetContainer(databaseId: DatabaseId, containerId: ContainerName);

            using FeedIterator<NewsArticleDTO> resultSet = container.GetItemQueryIterator<NewsArticleDTO>(query, null, null);

            if (resultSet.HasMoreResults)
            {
                FeedResponse<NewsArticleDTO> queryResponse = await resultSet.ReadNextAsync();
                NewsArticleDTO? articleResponse = queryResponse.FirstOrDefault();

                return articleResponse is null
                    ? null
                        : _dtoToEntityMapper.Map(articleResponse);
            }

            return null;
        }
        catch (CosmosException ex)
        {
            _logger.LogCritical(ex, "CosmosException in GetNewsArticleByIdAsync for id: {Id}", id);
            return null;
        }
    }

    public async Task<IEnumerable<NewsArticle>> GetNewsArticlesAsync(NewsArticleSearchFilter newsArticleSearchFilter)
    {
        try
        {
            string archivedFilter = string.Empty;
            string publishedFilter = string.Empty;

            switch (newsArticleSearchFilter)
            {
                case NewsArticleSearchFilter.ArchivedWithPublished:
                    archivedFilter = "c.Archived=true";
                    publishedFilter = " AND c.Published=true";
                    break;
                case NewsArticleSearchFilter.ArchivedWithNotPublished:
                    archivedFilter = "c.Archived=true";
                    publishedFilter = " AND c.Published=false";
                    break;
                case NewsArticleSearchFilter.ArchivedWithPublishedAndNotPublished:
                    archivedFilter = "c.Archived=true";
                    publishedFilter = string.Empty; // both published and not published
                    break;
                case NewsArticleSearchFilter.NotArchivedWithPublished:
                    archivedFilter = "c.Archived=false";
                    publishedFilter = " AND c.Published=true";
                    break;
                case NewsArticleSearchFilter.NotArchivedWithNotPublished:
                    archivedFilter = "c.Archived=false";
                    publishedFilter = " AND c.Published=false";
                    break;
                case NewsArticleSearchFilter.NotArchivedWithPublishedAndNotPublished:
                    archivedFilter = "c.Archived=false";
                    publishedFilter = string.Empty; // both published and not published
                    break;
            }

            string query = $"SELECT * FROM c WHERE c.DOCTYPE=7 AND {archivedFilter}{publishedFilter}";

            Container container = _cosmosClient.GetContainer(databaseId: DatabaseId, containerId: ContainerName);
            using FeedIterator<NewsArticleDTO> resultSet = container.GetItemQueryIterator<NewsArticleDTO>(query, null, null);

            List<NewsArticleDTO> responseArticles = [];
            while (resultSet.HasMoreResults)
            {
                FeedResponse<NewsArticleDTO> queryResponse = await resultSet.ReadNextAsync();
                responseArticles.AddRange(queryResponse);
            }

            return responseArticles.Select(_dtoToEntityMapper.Map);
        }
        catch (CosmosException ex)
        {
            _logger.LogCritical(ex, "CosmosException in GetNewsArticlesAsync.");
            return Enumerable.Empty<NewsArticle>();
        }
    }
}
