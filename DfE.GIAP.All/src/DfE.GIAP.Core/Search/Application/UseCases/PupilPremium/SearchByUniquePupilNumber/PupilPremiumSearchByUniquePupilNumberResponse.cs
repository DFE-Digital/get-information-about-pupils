using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;

namespace DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.SearchByUniquePupilNumber;
public record PupilPremiumSearchByUniquePupilNumberResponse
{
    internal PupilPremiumSearchByUniquePupilNumberResponse(PupilPremiumLearners? learners, int? totalResultsCount)
    {
        LearnerSearchResults = learners ?? PupilPremiumLearners.CreateEmpty();
        TotalNumberOfResults = new(totalResultsCount);
    }

    public PupilPremiumLearners LearnerSearchResults { get; }

    public SearchResultCount TotalNumberOfResults { get; }
}
