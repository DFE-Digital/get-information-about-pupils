using System.ComponentModel.DataAnnotations;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Sort;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;

namespace DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.SearchByUniqueLearnerNumber;
public record FurtherEducationSearchByUniqueLearnerNumberRequest : IUseCaseRequest<SearchResponse<FurtherEducationLearners>>
{
    public string[]? UniqueLearnerNumbers { get; init; }

    public SearchCriteria? SearchCriteria { get; init; }

    public SortOrder? Sort { get; init; }

    public int Offset { get; init; }
}
