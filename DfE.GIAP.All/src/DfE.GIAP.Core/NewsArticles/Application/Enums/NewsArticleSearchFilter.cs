namespace DfE.GIAP.Core.NewsArticles.Application.Enums;

/// <summary>
/// Specifies filters for searching news articles based on their archival and publication status.
/// </summary>
/// <remarks>This enumeration provides options to filter news articles by their archival state  (archived or not
/// archived) and publication status (published, not published, or both). Use these values to refine search results when
/// querying for news articles.</remarks>
public enum NewsArticleSearchFilter
{
    ArchivedWithPublished,
    ArchivedWithNotPublished,
    ArchivedWithPublishedAndNotPublished,
    NotArchivedWithPublished,
    NotArchivedWithNotPublished,
    NotArchivedWithPublishedAndNotPublished
}
