using DfE.GIAP.Core.NewsArticles.Application.Models;

namespace DfE.GIAP.Core.NewsArticles.Application.Repositories;

/// <summary>
/// Defines methods for creating and managing news articles in a data store.
/// </summary>
/// <remarks>This interface is intended for write operations related to news articles. Implementations should
/// handle persistence and validation of the provided data.</remarks>
public interface INewsArticleWriteRepository
{
    /// <summary>
    /// Asynchronously creates a new news article in the system.
    /// </summary>
    /// <param name="newsArticle">The news article to be created. Must not be null and must contain valid data.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task CreateNewsArticleAsync(NewsArticle newsArticle);


    /// <summary>
    /// Updates an existing news article with the provided details.
    /// </summary>
    /// <remarks>This method updates the news article in the underlying data store. Ensure that the provided 
    /// <paramref name="newsArticle"/> object contains valid and complete information before calling this
    /// method.</remarks>
    /// <param name="newsArticle">The <see cref="NewsArticle"/> object containing the updated details of the article. Must not be <see
    /// langword="null"/>.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateNewsArticleAsync(NewsArticle newsArticle);
}
