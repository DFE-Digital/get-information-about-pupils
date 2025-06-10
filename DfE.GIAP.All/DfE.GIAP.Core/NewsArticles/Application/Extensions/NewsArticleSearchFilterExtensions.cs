using DfE.GIAP.Core.NewsArticles.Application.Enums;

namespace DfE.GIAP.Core.NewsArticles.Application.Extensions;

public static class NewsArticleSearchFilterExtensions
{

    /// <summary>
    /// Converts a <see cref="NewsArticleSearchFilter"/> value into Cosmos DB filter expressions.
    /// </summary>
    /// <remarks>This method generates Cosmos DB filter expressions based on the provided <see
    /// cref="NewsArticleSearchFilter"/> value. The returned filters can be used to query Cosmos DB documents with
    /// specific "Archived" and "Published" field conditions.</remarks>
    /// <param name="filter">The <see cref="NewsArticleSearchFilter"/> value that specifies the desired filtering criteria.</param>
    /// <returns>A tuple containing two strings:  <list type="bullet"> <item> <description>The first string represents the filter
    /// for the "Archived" field.</description> </item> <item> <description>The second string represents the filter for
    /// the "Published" field, or an empty string if no specific "Published" filter is required.</description> </item>
    /// </list></returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="filter"/> value is not a valid <see cref="NewsArticleSearchFilter"/>.</exception>
    public static (string archivedFilter, string publishedFilter) ToCosmosFilters(this NewsArticleSearchFilter filter)
    {
        return filter switch
        {
            NewsArticleSearchFilter.ArchivedWithPublished =>
                ("c.Archived=true", " AND c.Published=true"),
            NewsArticleSearchFilter.ArchivedWithNotPublished =>
                ("c.Archived=true", " AND c.Published=false"),
            NewsArticleSearchFilter.ArchivedWithPublishedAndNotPublished =>
                ("c.Archived=true", string.Empty),
            NewsArticleSearchFilter.NotArchivedWithPublished =>
                ("c.Archived=false", " AND c.Published=true"),
            NewsArticleSearchFilter.NotArchivedWithNotPublished =>
                ("c.Archived=false", " AND c.Published=false"),
            NewsArticleSearchFilter.NotArchivedWithPublishedAndNotPublished =>
                ("c.Archived=false", string.Empty),
            _ => throw new ArgumentOutOfRangeException(nameof(filter), filter, null)
        };
    }
}
