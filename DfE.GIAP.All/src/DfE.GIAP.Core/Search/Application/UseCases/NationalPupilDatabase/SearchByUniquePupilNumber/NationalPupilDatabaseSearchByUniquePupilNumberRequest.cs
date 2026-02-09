using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Sort;

namespace DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.SearchByUniquePupilNumber;
public record NationalPupilDatabaseSearchByUniquePupilNumberRequest : IUseCaseRequest<NationalPupilDatabaseSearchByUniquePupilNumberResponse>
{
    public string[]? UniquePupilNumbers { get; init; }
    public SearchCriteria? SearchCriteria { get; init; }
    public SortOrder? Sort { get; init; }
    public int Offset { get; init; }
}
