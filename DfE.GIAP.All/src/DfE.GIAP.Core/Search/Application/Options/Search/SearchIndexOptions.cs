using DfE.GIAP.Web.Features.Search.Options.Search;
using DfE.GIAP.Web.Features.Search.Options.Sort;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DfE.GIAP.Core.Search.Application.Options.Search;
public sealed class SearchIndexOptions
{
    [JsonProperty(nameof(SearchCriteriaOptions))]
    public SearchCriteriaOptions? SearchCriteria { get; set; }

    [JsonProperty(nameof(SortOptions))]
    public SortOptions? SortOptions { get; set; }
}
