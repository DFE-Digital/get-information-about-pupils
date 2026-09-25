using DfE.GIAP.Core.NewsArticles.Application.Models;

namespace DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles;

/// <summary>
/// Represents the response containing a collection of news articles.
/// </summary>
/// <param name="NewsArticles">The list of retrieved news articles.</param>
public record GetNewsArticlesResponse(IEnumerable<NewsArticle> NewsArticles);
