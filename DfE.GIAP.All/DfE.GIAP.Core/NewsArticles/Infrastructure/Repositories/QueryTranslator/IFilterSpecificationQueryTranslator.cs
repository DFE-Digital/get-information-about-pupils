using DfE.GIAP.Core.Common.Application;

namespace DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories.QueryTranslator;
public interface IFilterSpecificationQueryTranslator<T>
{
    string TranslateSpecificationToQueryString(ISpecification<T> specification);
}
