﻿using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.NewsArticles.Application.Models;
using DfE.GIAP.Core.NewsArticles.Application.Repositories;

namespace DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles;

/// <summary>
/// Use case for retrieving a list of news articles.
/// </summary>
internal class GetNewsArticlesUseCase : IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse>
{
    /// <summary>
    /// Repository for reading news articles.
    /// </summary>
    private readonly INewsArticleReadRepository _newsArticleReadRepository;

    /// <summary>
    /// Initializes the use case with a news article repository.
    /// </summary>
    /// <param name="newsArticleReadRepository">Repository used to retrieve news articles.</param>
    /// <exception cref="ArgumentNullException">Thrown if the repository is null.</exception>
    public GetNewsArticlesUseCase(INewsArticleReadRepository newsArticleReadRepository)
    {
        _newsArticleReadRepository = newsArticleReadRepository ??
            throw new ArgumentNullException(nameof(newsArticleReadRepository));
    }

    /// <summary>
    /// Handles the request to retrieve news articles.
    /// </summary>
    /// <param name="request">Request containing filtering options.</param>
    /// <returns>Response containing a list of retrieved news articles.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the request is null.</exception>
    public async Task<GetNewsArticlesResponse> HandleRequest(GetNewsArticlesRequest request)
    {
        // Validate request object
        ArgumentNullException.ThrowIfNull(request);

        // Retrieve news articles based on the specified filters
        IEnumerable<NewsArticle> newsArticlesResult = await _newsArticleReadRepository.GetNewsArticlesAsync(request.IsArchived, request.IsDraft);

        // Order articles: Pinned first, then by last modified date in descending order
        IOrderedEnumerable<NewsArticle> orderedNewsArticles = newsArticlesResult
            .OrderByDescending(newsArticle => newsArticle.Pinned)
            .ThenByDescending(newsArticle => newsArticle.ModifiedDate);

        // Return the response containing the ordered list of articles
        return new GetNewsArticlesResponse(orderedNewsArticles);
    }
}
