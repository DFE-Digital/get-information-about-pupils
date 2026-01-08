using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.NewsArticles.Application.Repositories;

namespace DfE.GIAP.Core.NewsArticles.Application.UseCases.DeleteNewsArticle;

/// <summary>
/// Handles the deletion of a news article based on the provided request.
/// </summary>
/// <remarks>This use case is responsible for orchestrating the deletion of a news article by delegating the
/// operation to the appropriate repository. The request must include a valid article ID.</remarks>
public class DeleteNewsArticleUseCase : IUseCaseRequestOnly<DeleteNewsArticleRequest>
{
    private readonly INewsArticleWriteOnlyRepository _newsArticleWriteRepository;

    public DeleteNewsArticleUseCase(INewsArticleWriteOnlyRepository newsArticleWriteRepository)
    {
        ArgumentNullException.ThrowIfNull(newsArticleWriteRepository);
        _newsArticleWriteRepository = newsArticleWriteRepository;
    }

    /// <summary>
    /// Handles a request to delete a news article asynchronously.
    /// </summary>
    /// <param name="request">The request containing the details of the news article to delete.  The <see cref="DeleteNewsArticleRequest.Id"/>
    /// property must not be null.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task HandleRequestAsync(DeleteNewsArticleRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        await _newsArticleWriteRepository.DeleteNewsArticleAsync(request.Id);
    }
}
