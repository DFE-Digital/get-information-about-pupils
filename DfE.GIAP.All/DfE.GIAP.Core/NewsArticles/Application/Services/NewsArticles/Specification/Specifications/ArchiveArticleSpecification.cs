using System.Linq.Expressions;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.NewsArticles.Application.Models;

namespace DfE.GIAP.Core.NewsArticles.Application.Services.NewsArticles.Specification.Specifications;
public sealed class ArchivedArticleSpecification : ISpecification<NewsArticle>
{
    public ArchivedArticleSpecification(bool isArchived)
    {
        IsArchived = isArchived;
    }
    public bool IsArchived { get; }
    public bool IsSatisfiedBy(NewsArticle article) => ToExpression().Compile().Invoke(article);
    public Expression<Func<NewsArticle, bool>> ToExpression() => (article) => article.Archived == IsArchived;
}
