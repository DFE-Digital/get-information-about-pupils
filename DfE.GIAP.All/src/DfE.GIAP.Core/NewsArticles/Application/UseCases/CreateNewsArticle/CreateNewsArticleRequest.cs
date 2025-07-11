using DfE.GIAP.Core.Common.Application;

namespace DfE.GIAP.Core.NewsArticles.Application.UseCases.CreateNewsArticle;

/// <summary>
/// Represents a request to create a news article with specified content and publication settings.
/// </summary>
/// <remarks>This request is used to provide the necessary details for creating a news article, including its
/// title, body content, and publication preferences. The <see cref="Published"/> and <see cref="Pinned"/> properties
/// control the visibility and priority of the article.</remarks>
/// <param name="Title">The title of the news article. This value cannot be null or empty.</param>
/// <param name="Body">The main content of the news article. This value cannot be null or empty.</param>
/// <param name="Published">A value indicating whether the article should be published immediately. <see langword="true"/> to publish;
/// otherwise, <see langword="false"/>.</param>
/// <param name="Pinned">A value indicating whether the article should be pinned for higher visibility. <see langword="true"/> to pin the
/// article; otherwise, <see langword="false"/>.</param>
public record CreateNewsArticleRequest(string Title, string Body, bool Published, bool Pinned) : IUseCaseRequest;
