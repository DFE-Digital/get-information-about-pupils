using DfE.GIAP.Core.Search.Application.Models.Search;

namespace DfE.GIAP.Core.Search.Application.Options;
public sealed class SearchCriteriaOptions
{
    Dictionary<string, SearchCriteria> Criteria { get; set; } = [];
}
