﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
        _logger = logger ??
            throw new ArgumentNullException(nameof(logger));
        _cosmosDbCommandHandler = cosmosDbCommandHandler ??
            throw new ArgumentNullException(nameof(cosmosDbCommandHandler));
        _entityToDtoMapper = entityToDtoMapper ??
            throw new ArgumentNullException(nameof(entityToDtoMapper));
    }


    /// <summary>
    /// Creates a new news article in the database.
    /// </summary>
    /// <remarks>This method attempts to create a new news article in the database using the provided <see
    /// cref="NewsArticle"/> object. If a database error occurs, the method logs the error and returns <see
    /// langword="null"/>.</remarks>
    /// <param name="newsArticle">The <see cref="NewsArticle"/> object to be created. Must not be <see langword="null"/>.</param>
    /// <returns>The <see cref="NewsArticle"/> object that was successfully created, or <see langword="null"/> if the operation
    /// failed.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="newsArticle"/> is <see langword="null"/>.</exception>
    public async Task<NewsArticle?> CreateNewsArticleAsync(NewsArticle newsArticle)
    {
        try
        {
            if (newsArticle is null)
            {
                _logger.LogCritical("CreateNewsArticleAsync called with null or empty id.");
                throw new ArgumentException("NewsArticle must not be null or empty.", nameof(newsArticle));
            }

            NewsArticleDTO newsArticleDto = _entityToDtoMapper.Map(newsArticle);
            NewsArticleDTO _ = await _cosmosDbCommandHandler.CreateItemAsync(newsArticleDto, ContainerName, newsArticleDto.Id);

            return newsArticle;
        }
        catch (CosmosException ex)
        {
            _logger.LogCritical(ex, "CosmosDB error occurred while creating a news article: {Message}", ex.Message);
            return null;
        }
    }
}
