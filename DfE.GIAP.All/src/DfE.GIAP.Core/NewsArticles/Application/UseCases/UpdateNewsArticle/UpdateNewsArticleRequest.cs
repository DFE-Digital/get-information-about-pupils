using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.NewsArticles.Application.Models;

namespace DfE.GIAP.Core.NewsArticles.Application.UseCases.UpdateNewsArticle;

public record UpdateNewsArticleRequest(UpdateNewsArticlesRequestProperties UpdateArticleProperties) : IUseCaseRequest;

public record UpdateNewsArticlesRequestProperties
{
    public UpdateNewsArticlesRequestProperties(string id)
    {
        Id = NewsArticleIdentifier.From(id);
        DateTime updateDate = DateTime.UtcNow;
        ModifiedDate = updateDate;
        CreatedDate = updateDate;
    }

    public NewsArticleIdentifier Id { get; }
    public string? Title { get; init; }
    public string? Body { get; init; }
    public DateTime ModifiedDate { get; }
    public DateTime CreatedDate { get; }
    public bool? Pinned { get; init; }
    public bool? Published { get; init; }
}
