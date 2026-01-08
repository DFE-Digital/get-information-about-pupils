using DfE.GIAP.Core.NewsArticles.Application.Enums;

namespace DfE.GIAP.Core.NewsArticles.Application.Extensions;

public static class NewsArticleSearchFilterExtensions
{
    /// <summary>
    /// Converts a <see cref="NewsArticleSearchFilter"/> value into a Cosmos DB filter expression.
    /// </summary>
    /// <remarks>
    /// This method generates a Cosmos DB filter expression based on the provided <see cref="NewsArticleSearchFilter"/> value.
    /// The returned filter can be used to query Cosmos DB documents with specific "Archived" and "Published" field conditions.
    /// </remarks>
    /// <param name="filter">The <see cref="NewsArticleSearchFilter"/> value that specifies the desired filtering criteria.</param>
    /// <returns>
    /// A string representing the combined filter for the "Archived" and "Published" fields.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if the <paramref name="filter"/> value is not a valid <see cref="NewsArticleSearchFilter"/>.
    /// </exception>
    public static string ToCosmosFilters(this NewsArticleSearchFilter filter)
    {
        return filter switch
        {
            NewsArticleSearchFilter.Published =>
                "c.Published=true",
            NewsArticleSearchFilter.NotPublished =>
                "c.Published=false",
            NewsArticleSearchFilter.PublishedAndNotPublished =>
                "(c.Published=true OR c.Published=false)",
            _ => throw new ArgumentOutOfRangeException(nameof(filter), filter, null)
        };
    }
}
