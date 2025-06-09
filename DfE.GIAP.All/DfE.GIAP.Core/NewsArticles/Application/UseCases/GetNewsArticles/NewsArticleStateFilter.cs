namespace DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles;

public enum NewsArticleStateFilter
{
    ArchivedOnly,
    PublishedOnly,
    PublishedIncludeDrafts,
    NotArchived
}
