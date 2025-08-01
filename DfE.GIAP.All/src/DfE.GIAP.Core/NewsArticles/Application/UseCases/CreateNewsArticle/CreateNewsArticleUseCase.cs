using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.NewsArticles.Application.Models;
using DfE.GIAP.Core.NewsArticles.Application.Repositories;

namespace DfE.GIAP.Core.NewsArticles.Application.UseCases.CreateNewsArticle;

/// <summary>
/// Handles the creation of a new news article by processing the provided request.
/// </summary>
public class CreateNewsArticleUseCase : IUseCaseRequestOnly<CreateNewsArticleRequest>
{
    private readonly INewsArticleWriteOnlyRepository _newsArticleWriteRepository;

    public CreateNewsArticleUseCase(INewsArticleWriteOnlyRepository newsArticleWriteRepository)
    {
        ArgumentNullException.ThrowIfNull(newsArticleWriteRepository);
        _newsArticleWriteRepository = newsArticleWriteRepository;
    }

    /// <summary>
    /// Handles the creation of a news article based on the provided request.
    /// </summary>
    /// <param name="request">The request containing the details of the news article to be created.  The request must include a non-null,
    /// non-empty <see cref="CreateNewsArticleRequest.Title"/> and  <see cref="CreateNewsArticleRequest.Body"/>.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="request.Title"/> or <paramref name="request.Body"/> is <see langword="null"/>, empty,
    /// or consists only of whitespace.</exception>
    public async Task HandleRequestAsync(CreateNewsArticleRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Title);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Body);

        // TODO apply similar pattern UpdateNewsArticleRequestProperties
        NewsArticle newsArticle = new()
        {
            Id = NewsArticleIdentifier.New(),
            Title = request.Title,
            Body = request.Body,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow,
            Published = request.Published,
            Pinned = request.Pinned
        };

        await _newsArticleWriteRepository.CreateNewsArticleAsync(newsArticle);
    }
}
