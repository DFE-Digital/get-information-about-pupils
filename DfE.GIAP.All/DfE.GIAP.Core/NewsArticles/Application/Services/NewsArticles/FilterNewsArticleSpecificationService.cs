using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.NewsArticles.Application.Models;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles;

namespace DfE.GIAP.Core.NewsArticles.Application.Services.NewsArticles;
internal sealed class FilterNewsArticleSpecificationService : IFilterNewsArticleSpecificationService
{
    private readonly IFilterNewsArticleSpecificationFactory _factory;

    public FilterNewsArticleSpecificationService(IFilterNewsArticleSpecificationFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _factory = factory;
    }

    public ISpecification<NewsArticle> CreateSpecification(IEnumerable<NewsArticleStateFilter> state)
    {
        List<ISpecification<NewsArticle>> specificationFilters =
            state.Select(t => _factory.Create(t))
                .ToList();

        return specificationFilters.Count == 1 ?
            specificationFilters.Single() :
                specificationFilters.Aggregate((a, b) => new AndSpecificaton<NewsArticle>(a, b));
    }
}


internal interface IFilterNewsArticleSpecificationFactory
{
    ISpecification<NewsArticle> Create(NewsArticleStateFilter filter);
}

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
