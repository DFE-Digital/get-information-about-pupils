using DfE.GIAP.Core.Common.Application.Specification;
using DfE.GIAP.Core.Common.Application.Specification.Extensions;
using DfE.GIAP.Core.NewsArticles.Application.Models;
using DfE.GIAP.Core.NewsArticles.Application.Services.NewsArticles.Specification.Factory;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles;

namespace DfE.GIAP.Core.NewsArticles.Application.Services.NewsArticles.Specification;
internal sealed class NewsArticleSpecificationService : INewsArticleSpecificationService
{
    private readonly IFilterNewsArticleSpecificationFactory _factory;

    public NewsArticleSpecificationService(IFilterNewsArticleSpecificationFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _factory = factory;
    }

    public ISpecification<NewsArticle> CreateSpecification(IEnumerable<NewsArticleStateFilter> state)
    {
        List<ISpecification<NewsArticle>> specificationFilters =
            state.Select(_factory.Create)
                .ToList();

        return specificationFilters.Count == 1 ?
            specificationFilters.Single() :
                specificationFilters.Aggregate((a, b) => a.And(b));
    }
}
