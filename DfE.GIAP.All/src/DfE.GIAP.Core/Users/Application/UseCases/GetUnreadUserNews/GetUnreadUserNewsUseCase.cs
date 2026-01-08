using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.NewsArticles.Application.Repositories;
using DfE.GIAP.Core.Users.Application.Models;
using DfE.GIAP.Core.Users.Application.Repositories;

namespace DfE.GIAP.Core.Users.Application.UseCases.GetUnreadUserNews;

/// <summary>
/// Handles the retrieval of unread news articles for a specific user.
/// </summary>
/// <remarks>This use case determines whether there are any news articles that have been modified  since the
/// user's last login. It relies on read-only repositories for both user and  news article data.</remarks>
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

    /// <summary>
    /// Handles the request to determine if there are any unread news articles for a user.
    /// </summary>
    /// <param name="request">The request containing the user ID for which to check unread news articles.</param>
    /// <returns>A <see cref="GetUnreadUserNewsResponse"/> indicating whether any news articles have been updated  since the
    /// user's last login.</returns>
    public async Task<GetUnreadUserNewsResponse> HandleRequestAsync(GetUnreadUserNewsRequest request)
    {
        User user = await _userReadOnlyRepository
            .GetUserByIdAsync(new UserId(request.UserId));

        bool hasUpdated = await _newsArticleReadRepository
            .HasAnyNewsArticleBeenModifiedSinceAsync(user.LastLoggedIn);

        return new GetUnreadUserNewsResponse(hasUpdated);
    }
}
