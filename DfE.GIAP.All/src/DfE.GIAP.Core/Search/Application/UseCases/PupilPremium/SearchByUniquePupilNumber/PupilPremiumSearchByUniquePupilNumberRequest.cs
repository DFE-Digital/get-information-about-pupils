using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Sort;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;

namespace DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.SearchByUniquePupilNumber;
public record PupilPremiumSearchByUniquePupilNumberRequest : IUseCaseRequest<SearchResponse<PupilPremiumLearners>>
{
    public string[]? UniquePupilNumbers { get; init; }

    public SearchCriteria? SearchCriteria { get; init; }

    public SortOrder? Sort { get; init; }

    public int Offset { get; init; }
}
