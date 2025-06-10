using DfE.GIAP.Core.NewsArticles.Application.Enums;

namespace DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles;

/// <summary>
/// Represents a request to retrieve news articles based on specified search criteria.
/// </summary>
/// <remarks>This request encapsulates the search filter used to query news articles.  The filter defines the
/// criteria for narrowing down the results, such as keywords, date ranges, or categories.</remarks>
/// <param name="newsArticleSearchFilter">The filter specifying the search criteria for retrieving news articles.</param>
public record GetNewsArticlesRequest(NewsArticleSearchFilter newsArticleSearchFilter);
