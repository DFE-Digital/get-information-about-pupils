using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.NewsArticles.Application.Models;
using DfE.GIAP.Core.NewsArticles.Application.Repositories;

namespace DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticleById;

/// <summary>
/// Use case for retrieving a news article by its unique identifier.
/// </summary>
internal class GetNewsArticleByIdUseCase : IUseCase<GetNewsArticleByIdRequest, GetNewsArticleByIdResponse>
{
    /// <summary>
    /// Repository for reading news articles.
    /// </summary>
    private readonly INewsArticleReadRepository _newsArticleReadRepository;

    /// <summary>
    /// Initializes the use case with a news article repository.
    /// </summary>
    /// <param name="newsArticleReadRepository">Repository used to retrieve news articles.</param>
    /// <exception cref="ArgumentNullException">Thrown when repository is null.</exception>
    public GetNewsArticleByIdUseCase(INewsArticleReadRepository newsArticleReadRepository)
    {
        _newsArticleReadRepository = newsArticleReadRepository ??
            throw new ArgumentNullException(nameof(newsArticleReadRepository));
    }

    /// <summary>
    /// Handles the request to retrieve a news article.
    /// </summary>
    /// <param name="request">Request containing the article ID.</param>
    /// <returns>Response containing the retrieved news article.</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    /// <exception cref="ArgumentException">Thrown when request ID is invalid.</exception>
    public async Task<GetNewsArticleByIdResponse> HandleRequestAsync(GetNewsArticleByIdRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Id))
        {
            throw new ArgumentException("News Article Id cannot be null or empty.");
        }

        // Retrieve the news article from the repository
        NewsArticle? newsArticleResult = await _newsArticleReadRepository.GetNewsArticleByIdAsync(request.Id);

        // Return the response containing the retrieved article
        return new GetNewsArticleByIdResponse(newsArticleResult);
    }
}
