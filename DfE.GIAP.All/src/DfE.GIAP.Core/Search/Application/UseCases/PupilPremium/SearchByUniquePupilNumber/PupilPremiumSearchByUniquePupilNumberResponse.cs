using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;

namespace DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.SearchByUniquePupilNumber;
public record PupilPremiumSearchByUniquePupilNumberResponse
{
    internal PupilPremiumSearchByUniquePupilNumberResponse(PupilPremiumLearners? learners, SearchResultCount searchResultCount)
    {
        LearnerSearchResults = learners ?? PupilPremiumLearners.CreateEmpty();
        TotalNumberOfResults = searchResultCount;
    }

    public PupilPremiumLearners LearnerSearchResults { get; }

    public SearchResultCount TotalNumberOfResults { get; }
}
