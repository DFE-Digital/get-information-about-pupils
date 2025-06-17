using DfE.GIAP.Core.Common.Application;

namespace DfE.GIAP.Core.NewsArticles.Application.UseCases.CreateNewsArticle;

/// <summary>
/// Represents a request to create a news article with specified attributes.
/// </summary>
/// <param name="Title">The title of the news article. Cannot be null or empty.</param>
/// <param name="Body">The body content of the news article. Cannot be null or empty.</param>
/// <param name="Published">A value indicating whether the news article is published. <see langword="true"/> if published; otherwise, <see
/// langword="false"/>.</param>
/// <param name="Archived">A value indicating whether the news article is archived. <see langword="true"/> if archived; otherwise, <see
/// langword="false"/>.</param>
/// <param name="Pinned">A value indicating whether the news article is pinned. <see langword="true"/> if pinned; otherwise, <see
/// langword="false"/>.</param>
public record CreateNewsArticleRequest(string Title, string Body, bool Published, bool Archived, bool Pinned) : IUseCaseRequest;
