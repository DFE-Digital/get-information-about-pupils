using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.SearchByUniqueLearnerNumber;

namespace DfE.GIAP.Web.Tests.Features.Search.FurtherEducation.SearchByUniqueLearnerNumber;
public static class FurtherEducationSearchByUniqueLearnerNumberTestDouble
{
    
    public static FurtherEducationSearchByUniqueLearnerNumberResponse Create(
        FurtherEducationLearners learners,
        int? totalResults = null) =>
            new(learners, totalResults ?? learners.Count);

    public static FurtherEducationSearchByUniqueLearnerNumberResponse CreateSuccessResponse()
    {
        // Construct a sample learner with basic identity and characteristics
        FurtherEducationLearners learners = new(
            [new(
                new FurtherEducationLearnerIdentifier("1234567890"),
                new LearnerName("Alice", "Smith"),
                new LearnerCharacteristics(
                    new DateTime(2005, 6, 1),
                    Sex.Female)
                )
            ]
        );

        // Return a success response with the sample learner and facet
        return new FurtherEducationSearchByUniqueLearnerNumberResponse(learners, learners.Count);
    }
}
