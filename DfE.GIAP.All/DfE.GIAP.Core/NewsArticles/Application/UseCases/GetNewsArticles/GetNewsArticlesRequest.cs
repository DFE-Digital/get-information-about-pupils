namespace DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles;

/// <summary>
/// Represents a request for retrieving news articles.
/// </summary>
/// <param name="IsArchived">Indicates whether to fetch archived news articles.</param>
/// <param name="IsPublished">
/// Indicates whether to fetch published news articles. 
/// If null, fetches both published and unpublished articles.
/// </param>
public record GetNewsArticlesRequest(bool IsArchived, bool? IsPublished);
