using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.NewsArticles.Application.Repositories;
using DfE.GIAP.Core.Users.Application;
using DfE.GIAP.Core.Users.Application.Repository;

namespace DfE.GIAP.Core.NewsArticles.Application.UseCases.CheckNewsArticleUpdates;
public class CheckNewsArticleUpdatesUseCase : IUseCase<CheckNewsArticleUpdatesRequest, CheckNewsArticleUpdateResponse>
{
    private readonly INewsArticleReadOnlyRepository _newsArticleReadRepository;
    private readonly IUserReadOnlyRepository _userReadOnlyRepository;

    public CheckNewsArticleUpdatesUseCase(
        INewsArticleReadOnlyRepository newsArticleReadRepository,
        IUserReadOnlyRepository userReadOnlyRepository)
    {
        ArgumentNullException.ThrowIfNull(newsArticleReadRepository);
        _newsArticleReadRepository = newsArticleReadRepository;

        ArgumentNullException.ThrowIfNull(userReadOnlyRepository);
        _userReadOnlyRepository = userReadOnlyRepository;
    }


    public async Task<CheckNewsArticleUpdateResponse> HandleRequestAsync(CheckNewsArticleUpdatesRequest request)
    {
        User user = await _userReadOnlyRepository
            .GetUserByIdAsync(new UserId(request.UserId));

        bool hasUpdated = await _newsArticleReadRepository
            .HasAnyNewsArticleBeenModifiedSinceAsync(user.LatestNewsAccessedDateTime);

        return new CheckNewsArticleUpdateResponse(hasUpdated);
    }
}
