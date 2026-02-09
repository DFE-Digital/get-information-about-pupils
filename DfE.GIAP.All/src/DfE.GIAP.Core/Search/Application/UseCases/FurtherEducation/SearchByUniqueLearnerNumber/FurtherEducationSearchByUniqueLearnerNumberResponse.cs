using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;

namespace DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.SearchByUniqueLearnerNumber;
public record FurtherEducationSearchByUniqueLearnerNumberResponse
{
    public FurtherEducationSearchByUniqueLearnerNumberResponse(FurtherEducationLearners? learners, int? searchResultCount)
    {
        LearnerSearchResults = learners ?? FurtherEducationLearners.CreateEmpty();
        TotalNumberOfResults = new(searchResultCount);
    }
    
    public FurtherEducationLearners LearnerSearchResults { get; }

    public SearchResultCount TotalNumberOfResults { get; }
}
