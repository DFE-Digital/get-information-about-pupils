using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.NewsArticles.Application.Models;
using DfE.GIAP.Core.NewsArticles.Application.Services.NewsArticles.Specification;

namespace DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories.QueryTranslator;
internal sealed class NewsArticleFilterSpecificationQueryTranslator : IFilterSpecificationQueryTranslator<NewsArticle>
{
    public string TranslateSpecificationToQueryString(ISpecification<NewsArticle> specification)
    {
        // Type match each specification to translate to a SQL Query clause
        string filter = specification switch
        {
            PublishedArticleSpecification published =>
                published.IncludeOnlyPublished ?
                    $"c.Published = true" :
                        string.Empty,

            ArchivedArticleSpecification archived =>
                $"c.IsArchived = {(archived.IsArchived ? "true" : "false")}",

            AndSpecificaton<NewsArticle> andSpec
                // to avoid "" AND or AND ""
                when string.IsNullOrEmpty(TranslateSpecificationToQueryString(andSpec.Left)) && string.IsNullOrEmpty(TranslateSpecificationToQueryString(andSpec.Right))
                    => string.Empty,

            // to avoid "" AND c.Property= 
            AndSpecificaton<NewsArticle> andSpec
                when string.IsNullOrEmpty(TranslateSpecificationToQueryString(andSpec.Left))
                    => TranslateSpecificationToQueryString(andSpec.Right),

            // to avoid "" AND c.Property= 
            AndSpecificaton<NewsArticle> andSpec
                when string.IsNullOrEmpty(TranslateSpecificationToQueryString(andSpec.Right))
                    => TranslateSpecificationToQueryString(andSpec.Left),

            AndSpecificaton<NewsArticle> andSpec
                => $"{TranslateSpecificationToQueryString(andSpec.Left)} AND {TranslateSpecificationToQueryString(andSpec.Right)}",

            _ => throw new NotSupportedException($"Specification type {specification.GetType().Name} is not supported.")
        };

        string output = $"SELECT * FROM c WHERE c.DOCTYPE = 7 {(string.IsNullOrEmpty(filter) ? string.Empty : $"AND {filter}")}";
        return output;
    }
}
