using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.NewsArticles.Application.Models;
using DfE.GIAP.Core.NewsArticles.Application.Repositories;

namespace DfE.GIAP.Core.NewsArticles.Application.UseCases.CreateNewsArticle;

/// <summary>
/// Handles the creation of a new news article by processing the provided request.
/// </summary>
public class CreateNewsArticleUseCase : IUseCaseRequestOnly<CreateNewsArticleRequest>
{
    /// <summary>
    /// Provides access to operations for writing news articles to the data store.
    /// </summary>
    /// <remarks>This repository is used to perform create, update, and delete operations on news articles. It
    /// is intended for use in scenarios where persistence of news article data is required.</remarks>
    private readonly INewsArticleWriteRepository _newsArticleWriteRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateNewsArticleUseCase"/> class.
    /// </summary>
    /// <param name="newsArticleWriteRepository">The repository used to persist news articles. Cannot be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="newsArticleWriteRepository"/> is <see langword="null"/>.</exception>
    public CreateNewsArticleUseCase(INewsArticleWriteRepository newsArticleWriteRepository)
    {
        _newsArticleWriteRepository = newsArticleWriteRepository ??
            throw new ArgumentNullException(nameof(newsArticleWriteRepository));
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

        NewsArticle newsArticle = new()
        {
            Id = NewsArticleIdentifier.New(),
            Title = request.Title,
            Body = request.Body,
            DraftBody = string.Empty,
            DraftTitle = string.Empty,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow,
            Published = request.Published,
            Archived = request.Archived,
            Pinned = request.Pinned
        };

        await _newsArticleWriteRepository.CreateNewsArticleAsync(newsArticle);
    }
}
