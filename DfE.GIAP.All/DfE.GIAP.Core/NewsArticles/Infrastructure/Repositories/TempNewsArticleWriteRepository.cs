// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.NewsArticles.Application.Models;
using DfE.GIAP.Core.NewsArticles.Application.Repositories;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories;
internal class TempNewsArticleWriteRepository : INewsArticleWriteRepository
{
    private const string ContainerName = "application-data";
    private const string DatabaseId = "giapsearch";
    private readonly ILogger<TempNewsArticleWriteRepository> _logger;
    private readonly CosmosClient _cosmosClient;
    private readonly IMapper<NewsArticle, NewsArticleDTO> _entityToDtoMapper;

    public TempNewsArticleWriteRepository(
        ILogger<TempNewsArticleWriteRepository> logger,
        CosmosClient cosmosClient,
        IMapper<NewsArticle, NewsArticleDTO> entityToDtoMapper)
    {
        _logger = logger ??
            throw new ArgumentNullException(nameof(logger));
        _cosmosClient = cosmosClient ??
            throw new ArgumentNullException(nameof(cosmosClient));
        _entityToDtoMapper = entityToDtoMapper ??
            throw new ArgumentNullException(nameof(entityToDtoMapper));
    }

    public async Task<NewsArticle?> CreateNewsArticleAsync(NewsArticle newsArticle)
    {
        try
        {
            if (newsArticle is null)
            {
                _logger.LogCritical("CreateNewsArticleAsync called with null.");
                throw new ArgumentNullException("NewsArticle must not be null.", nameof(newsArticle));
            }

            NewsArticleDTO newsArticleDto = _entityToDtoMapper.Map(newsArticle);

            Container container = _cosmosClient.GetContainer(databaseId: DatabaseId, containerId: ContainerName);
            ItemResponse<NewsArticleDTO> response = await container.CreateItemAsync(newsArticleDto, new PartitionKey(newsArticleDto.Id));

            return newsArticle;
        }
        catch (CosmosException ex)
        {
            _logger.LogCritical(ex, "CosmosDB error occurred while creating a news article: {Message}", ex.Message);
            return null;
        }
    }
}
