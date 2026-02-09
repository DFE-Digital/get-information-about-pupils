using System.ComponentModel.DataAnnotations;
using DfE.GIAP.Core.Search.Application.Models.Filter;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Sort;

namespace DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.SearchByName;

public record FurtherEducationSearchByNameRequest : IUseCaseRequest<FurtherEducationSearchByNameResponse>
{
    public string? SearchKeywords { get; init; }
    
    public SearchCriteria? SearchCriteria { get; init; }
    
    public SortOrder? SortOrder { get; init; }
    
    public IList<FilterRequest>? FilterRequests { get; init; }

    public int Offset { get; init; }
}
