using DfE.GIAP.Core.Search.Application.Models.Filter;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Sort;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;

namespace DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.SearchByName;
public record NationalPupilDatabaseSearchByNameRequest : IUseCaseRequest<SearchResponse<NationalPupilDatabaseLearners>>
{
    public string? SearchKeywords { get; init; }
    public SearchCriteria? SearchCriteria { get; init; }
    public SortOrder? SortOrder { get; init; }
    public int Offset { get; init; }
    public IList<FilterRequest>? FilterRequests { get; init; }
}
