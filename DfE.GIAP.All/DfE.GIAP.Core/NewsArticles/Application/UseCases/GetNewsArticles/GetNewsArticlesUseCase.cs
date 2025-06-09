using DfE.GIAP.Core.Common.Application;
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
        IEnumerable<NewsArticle> newsArticlesResult = await _newsArticleReadRepository
            .GetNewsArticlesAsync(newsArticleSearchStatus: request.searchStatus);

        // Order articles based on the request's IsArchived flag
        IOrderedEnumerable<NewsArticle> orderedNewsArticles;
        switch (request.searchStatus)
        {
            case NewsArticleSearchStatus.ArchivedWithPublished:
            case NewsArticleSearchStatus.ArchivedWithNotPublished:
            case NewsArticleSearchStatus.ArchivedWithPublishedAndNotPublished:
                orderedNewsArticles = newsArticlesResult.OrderByDescending(x => x.ModifiedDate);
                break;
            case NewsArticleSearchStatus.NotArchivedWithPublished:
            case NewsArticleSearchStatus.NotArchivedWithNotPublished:
            case NewsArticleSearchStatus.NotArchivedWithPublishedAndNotPublished:
                orderedNewsArticles = newsArticlesResult
                    .OrderByDescending(x => x.Pinned)
                    .ThenByDescending(x => x.ModifiedDate);
                break;
            default:
                orderedNewsArticles = newsArticlesResult.OrderByDescending(x => x.ModifiedDate);
                break;
        }

        // Return the response containing the ordered list of articles
        return new GetNewsArticlesResponse(orderedNewsArticles);
    }
}
