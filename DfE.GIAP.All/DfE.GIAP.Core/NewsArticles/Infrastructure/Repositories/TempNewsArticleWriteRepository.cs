﻿using DfE.GIAP.Core.Common.CrossCutting;
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
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(cosmosClient);
        ArgumentNullException.ThrowIfNull(entityToDtoMapper);
        _logger = logger;
        _cosmosClient = cosmosClient;
        _entityToDtoMapper = entityToDtoMapper;
    }

    public async Task CreateNewsArticleAsync(NewsArticle newsArticle)
    {
        ArgumentNullException.ThrowIfNull(newsArticle);
        ArgumentException.ThrowIfNullOrWhiteSpace(newsArticle.Title);
        ArgumentException.ThrowIfNullOrWhiteSpace(newsArticle.Body);

        try
        {
            NewsArticleDTO newsArticleDto = _entityToDtoMapper.Map(newsArticle);

            Container container = _cosmosClient.GetContainer(databaseId: DatabaseId, containerId: ContainerName);
            await container.CreateItemAsync(newsArticleDto, new PartitionKey(7));
        }
        catch (CosmosException ex)
        {
            _logger.LogCritical(ex, "CosmosDB error occurred while creating a news article: {Message}", ex.Message);
            throw;
        }
    }
}
