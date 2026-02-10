using DfE.GIAP.Web.Features.Search.Options.Search;
using DfE.GIAP.Web.Features.Search.Options.Sort;

namespace DfE.GIAP.Core.Search.Application.Options.Search;
public sealed class SearchIndexOptions
{
    public SearchCriteriaOptions? SearchCriteria { get; set; }
    public SortOptions? SortOptions { get; set; }
}
