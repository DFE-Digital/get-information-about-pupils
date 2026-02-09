using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Web.Features.Search.Options.Search;

namespace DfE.GIAP.Core.Search.Application.Options.Search;
internal sealed class SearchCriteriaOptionsToSearchCriteriaMapper : IMapper<SearchCriteriaOptions, SearchCriteria>
{
    public SearchCriteria Map(SearchCriteriaOptions input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return new()
        {
            Index = input.SearchIndex,
            Facets = input.Facets ?? [],
            SearchFields = input.SearchFields ?? [],
            Size = input.Size,
            IncludeTotalCount = input.IncludeTotalCount
        };
    }
}
