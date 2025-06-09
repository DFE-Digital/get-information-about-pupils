using System.Linq.Expressions;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.NewsArticles.Application.Models;

namespace DfE.GIAP.Core.NewsArticles.Application.Services.NewsArticles.Specification;
public sealed class ArchivedArticleSpecification : ISpecification<NewsArticle>
{
    private readonly bool _isArchived;

    public ArchivedArticleSpecification(bool isArchived)
    {
        _isArchived = isArchived;
    }
    public bool IsArchived => _isArchived;
    public bool IsSatisfiedBy(NewsArticle article) => ToExpression().Compile().Invoke(article);
    public Expression<Func<NewsArticle, bool>> ToExpression() => (article) => article.Archived == _isArchived;
}
