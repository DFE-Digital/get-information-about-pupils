using DfE.GIAP.Core.Search.Application.Models.Filter;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Sort;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;

namespace DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.SearchByName;
public sealed class PupilPremiumSearchByNameRequest : IUseCaseRequest<SearchResponse<PupilPremiumLearners>>
{
    public string? SearchKeywords { get; init; }
    public SearchCriteria? SearchCriteria { get; init; }
    public SortOrder? SortOrder { get; init; }
    public int Offset { get; init; }
    public IList<FilterRequest>? FilterRequests { get; init; }
}
