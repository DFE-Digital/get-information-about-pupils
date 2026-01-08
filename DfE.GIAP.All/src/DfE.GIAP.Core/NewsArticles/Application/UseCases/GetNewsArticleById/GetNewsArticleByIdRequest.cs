using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.NewsArticles.Application.Models;

namespace DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticleById;

/// <summary>
/// Represents a request to retrieve a news article by its unique identifier.
/// </summary>
/// <param name="Id">The unique identifier of the news article.</param>
public record GetNewsArticleByIdRequest(NewsArticleIdentifier Id) : IUseCaseRequest<GetNewsArticleByIdResponse>;
