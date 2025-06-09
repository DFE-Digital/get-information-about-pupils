using DfE.GIAP.Core.Common.Application.Specification;
using DfE.GIAP.Core.NewsArticles.Application.Models;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles;

namespace DfE.GIAP.Core.NewsArticles.Application.Services.NewsArticles.Specification.Factory;
internal class FilterNewsArticleSpecificationFactory : IFilterNewsArticleSpecificationFactory
{
    private readonly Dictionary<NewsArticleStateFilter, Func<ISpecification<NewsArticle>>> _map;

    public FilterNewsArticleSpecificationFactory(Dictionary<NewsArticleStateFilter, Func<ISpecification<NewsArticle>>> map)
    {
        ArgumentNullException.ThrowIfNull(map);
        if (map.Count == 0)
        {
            throw new ArgumentException($"Configuration is empty for {nameof(FilterNewsArticleSpecificationFactory)}");
        }
        _map = map;
    }

    public ISpecification<NewsArticle> Create(NewsArticleStateFilter filter)
    {
        if (!_map.TryGetValue(filter, out Func<ISpecification<NewsArticle>>? specificationHandler))
        {
            throw new ArgumentException($"unable to find state {filter}");
        }
        return specificationHandler.Invoke();
    }
}
