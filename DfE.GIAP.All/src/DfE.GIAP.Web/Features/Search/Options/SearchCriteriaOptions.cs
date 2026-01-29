using DfE.GIAP.Core.Search.Application.Models.Search;

namespace DfE.GIAP.Web.Features.Search.Options;
public sealed class SearchCriteriaOptions
{
    public Dictionary<string, SearchCriteria> Criteria { get; set; } = [];
}
