using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.NewsArticles.Application.Repositories;

namespace DfE.GIAP.Core.NewsArticles.Application.UseCases.CheckNewsArticleUpdates;
public class CheckNewsArticleUpdatesUseCase : IUseCase<CheckNewsArticleUpdatesRequest, CheckNewsArticleUpdateResponse>
{
    private readonly INewsArticleReadRepository _newsArticleReadRepository;
    //private readonly IUserReadRepository _userReadOnlyRepository;
    public CheckNewsArticleUpdatesUseCase(INewsArticleReadRepository newsArticleReadRepository)
    {
        ArgumentNullException.ThrowIfNull(newsArticleReadRepository);
        _newsArticleReadRepository = newsArticleReadRepository;
    }


    public async Task<CheckNewsArticleUpdateResponse> HandleRequestAsync(CheckNewsArticleUpdatesRequest request)
    {

        // TODO: Get user By Id
        // Pass User LastAccessedDateTime to the repository

        bool hasUpdated = await _newsArticleReadRepository.HasArticlesBeenModifiedSinceAsync(DateTime.UtcNow.AddMinutes(-5));

        return new CheckNewsArticleUpdateResponse(hasUpdated);
    }
}
