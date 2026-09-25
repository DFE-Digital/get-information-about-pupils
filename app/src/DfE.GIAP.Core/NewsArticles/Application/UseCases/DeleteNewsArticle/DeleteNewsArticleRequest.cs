using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.NewsArticles.Application.Models;

namespace DfE.GIAP.Core.NewsArticles.Application.UseCases.DeleteNewsArticle;

/// <summary>
/// Represents a request to delete a news article.
/// </summary>
/// <remarks>This request encapsulates the identifier of the news article to be deleted. It is typically used in
/// scenarios where a client needs to remove an existing article from a data store or system.</remarks>
/// <param name="Id">The unique identifier of the news article to be deleted. This parameter must not be null.</param>
public record DeleteNewsArticleRequest(NewsArticleIdentifier Id) : IUseCaseRequest;
