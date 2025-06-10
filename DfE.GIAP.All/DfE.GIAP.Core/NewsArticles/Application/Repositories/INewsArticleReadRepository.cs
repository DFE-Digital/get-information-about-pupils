using DfE.GIAP.Core.NewsArticles.Application.Enums;
using DfE.GIAP.Core.NewsArticles.Application.Models;

namespace DfE.GIAP.Core.NewsArticles.Application.Repositories;


/// <summary>
/// Defines a contract for reading news article data from a data source.
/// </summary>
/// <remarks>
/// This interface provides asynchronous methods for retrieving news articles, either as a collection filtered by 
/// publication status or individually by unique identifier. It is intended to be implemented by classes that 
/// encapsulate data access logic, such as repositories interacting with databases, in-memory stores, or external APIs.
/// </remarks>
public interface INewsArticleReadRepository
{
    /// <summary>
    /// Asynchronously retrieves a collection of news articles based on the specified search filter.
    /// </summary>
    /// <param name="newsArticleSearchFilter">The filter criteria used to search for news articles. This includes parameters such as keywords, date ranges, and
    /// categories.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of <see
    /// cref="NewsArticle"/> objects matching the search criteria. If no articles match the filter, the collection will
    /// be empty.</returns>
    Task<IEnumerable<NewsArticle>> GetNewsArticlesAsync(NewsArticleSearchFilter newsArticleSearchFilter);

    /// <summary>
    /// Asynchronously retrieves a news article by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the news article to retrieve. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the  <see cref="NewsArticle"/>
    /// if found; otherwise, <see langword="null"/>.</returns>
    /// <exception cref="ArgumentException">Thrown if the id is null or empty.</exception>
    Task<NewsArticle?> GetNewsArticleByIdAsync(string id);
}
