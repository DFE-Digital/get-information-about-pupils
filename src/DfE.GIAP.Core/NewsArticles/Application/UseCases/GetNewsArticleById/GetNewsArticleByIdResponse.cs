using DfE.GIAP.Core.NewsArticles.Application.Models;

namespace DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticleById;

/// <summary>
/// Represents the response containing a news article retrieved by its unique identifier.
/// </summary>
/// <param name="NewsArticle">The retrieved news article; null if not found.</param>
public record GetNewsArticleByIdResponse(NewsArticle? NewsArticle);
