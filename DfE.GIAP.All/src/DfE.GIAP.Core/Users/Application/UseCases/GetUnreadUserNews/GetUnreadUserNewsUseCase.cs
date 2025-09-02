using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.NewsArticles.Application.Repositories;
using DfE.GIAP.Core.Users.Application.Repositories;

namespace DfE.GIAP.Core.Users.Application.UseCases.GetUnreadUserNews;
public class GetUnreadUserNewsUseCase : IUseCase<GetUnreadUserNewsRequest, GetUnreadUserNewsResponse>
{
    private readonly INewsArticleReadOnlyRepository _newsArticleReadRepository;
    private readonly IUserReadOnlyRepository _userReadOnlyRepository;
    public GetUnreadUserNewsUseCase(
        INewsArticleReadOnlyRepository newsArticleReadRepository,
        IUserReadOnlyRepository userReadOnlyRepository)
    {
        ArgumentNullException.ThrowIfNull(newsArticleReadRepository);
        _newsArticleReadRepository = newsArticleReadRepository;
        ArgumentNullException.ThrowIfNull(userReadOnlyRepository);
        _userReadOnlyRepository = userReadOnlyRepository;
    }
    public async Task<GetUnreadUserNewsResponse> HandleRequestAsync(GetUnreadUserNewsRequest request)
    {
        User user = await _userReadOnlyRepository
            .GetUserByIdAsync(new UserId(request.UserId));

        bool hasUpdated = await _newsArticleReadRepository
            .HasAnyNewsArticleBeenModifiedSinceAsync(user.LastLoggedIn);

        return new GetUnreadUserNewsResponse(hasUpdated);
    }
}
