using System.Linq.Expressions;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.NewsArticles.Application.Models;

namespace DfE.GIAP.Core.NewsArticles.Application.Services.NewsArticles.Specification;
public sealed class PublishedArticleSpecification : ISpecification<NewsArticle>
{
    private readonly bool _includeOnlyPublished;

    public PublishedArticleSpecification(bool includeOnlyPublished)
    {
        _includeOnlyPublished = includeOnlyPublished;
    }

    public bool IncludeOnlyPublished => _includeOnlyPublished;

    public bool IsSatisfiedBy(NewsArticle article) => ToExpression().Compile().Invoke(article);
    public Expression<Func<NewsArticle, bool>> ToExpression() => (article) => !_includeOnlyPublished || article.Published;
}
