using DfE.GIAP.Core.Common.Application;

namespace DfE.GIAP.Core.NewsArticles.Application.UseCases.CheckNewsArticleUpdates;

public record CheckNewsArticleUpdatesRequest(string UserId) : IUseCaseRequest<CheckNewsArticleUpdateResponse>;
