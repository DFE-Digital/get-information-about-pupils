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
}
