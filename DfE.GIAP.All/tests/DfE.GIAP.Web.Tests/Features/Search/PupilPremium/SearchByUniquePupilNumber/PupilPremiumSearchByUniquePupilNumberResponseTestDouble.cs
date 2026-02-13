using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.SearchByName;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.SearchByUniquePupilNumber;
using DfE.GIAP.SharedTests.TestDoubles.Learner;
using DfE.GIAP.SharedTests.TestDoubles.SearchIndex;

namespace DfE.GIAP.Web.Tests.Features.Search.PupilPremium.SearchByUniquePupilNumber;
internal static class PupilPremiumSearchByUniquePupilNumberResponseTestDouble
{

    public static PupilPremiumSearchByUniquePupilNumberResponse Create(
        PupilPremiumLearners learners,
        int? totalResults = null) =>
            new(learners, totalResults ?? learners.Count);

    public static PupilPremiumSearchByUniquePupilNumberResponse CreateSuccessResponse()
    {
        // Construct a sample learner with basic identity and characteristics
        PupilPremiumLearners learners = new(PupilPremiumLearnerTestDoubles.FakeMany());

        // Return a success response with the sample learner and facet
        return new PupilPremiumSearchByUniquePupilNumberResponse(learners, learners.Count);
    }
}
