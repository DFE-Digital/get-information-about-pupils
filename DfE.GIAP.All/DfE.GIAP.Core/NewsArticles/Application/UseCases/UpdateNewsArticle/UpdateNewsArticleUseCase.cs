using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.NewsArticles.Application.Repositories;

namespace DfE.GIAP.Core.NewsArticles.Application.UseCases.UpdateNewsArticle;

public class UpdateNewsArticleUseCase : IUseCaseRequestOnly<UpdateNewsArticleRequest>
{
    private readonly INewsArticleWriteRepository _newsArticleWriteRepository;
    public UpdateNewsArticleUseCase(INewsArticleWriteRepository newsArticleWriteRepository)
    {
        ArgumentNullException.ThrowIfNull(newsArticleWriteRepository);
        _newsArticleWriteRepository = newsArticleWriteRepository;
    }

    public async Task HandleRequestAsync(UpdateNewsArticleRequest request)
    {
        // validate request
        ArgumentNullException.ThrowIfNull(request);

        // Update news article modified date
        await _newsArticleWriteRepository.UpdateNewsArticleAsync(request.NewsArticle);
    }
}
