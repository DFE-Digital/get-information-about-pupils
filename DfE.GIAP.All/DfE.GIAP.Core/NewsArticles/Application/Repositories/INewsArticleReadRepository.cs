using DfE.GIAP.Core.Common.CrossCutting;
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
    /// Retrieves a collection of news articles based on their publication status.
    /// </summary>
    /// <remarks>The method filters news articles based on the provided parameters. If both <paramref
    /// name="isArchived"/>  and <paramref name="isDraft"/> are <see langword="false"/>, only published articles
    /// will be retrieved.</remarks>
    /// <param name="isArchived">A value indicating whether to include archived articles.  Specify <see langword="true"/> to retrieve
    /// archived articles; otherwise, <see langword="false"/>.</param>
    /// <param name="isDraft">A value indicating whether to include draft articles.  Specify <see langword="true"/> to retrieve draft
    /// articles; otherwise, <see langword="false"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an  <see cref="IEnumerable{T}"/>
    /// of <see cref="NewsArticle"/> objects that match the specified criteria.</returns>
    Task<IEnumerable<NewsArticle>> GetNewsArticlesAsync(IFilterSpecification<NewsArticle> filter);

    /// <summary>
    /// Asynchronously retrieves a news article by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the news article to retrieve. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the  <see cref="NewsArticle"/>
    /// if found; otherwise, <see langword="null"/>.</returns>
    /// <exception cref="ArgumentException">Thrown if the id is null or empty.</exception>
    Task<NewsArticle?> GetNewsArticleByIdAsync(string id);
}
