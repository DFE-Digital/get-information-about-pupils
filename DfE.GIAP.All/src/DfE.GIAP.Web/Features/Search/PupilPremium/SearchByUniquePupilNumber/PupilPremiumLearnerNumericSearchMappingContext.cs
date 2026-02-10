using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.SearchByUniquePupilNumber;
using DfE.GIAP.Web.ViewModels.Search;

namespace DfE.GIAP.Web.Features.Search.PupilPremium.SearchByUniquePupilNumber;

public record PupilPremiumLearnerNumericSearchMappingContext
{
    public LearnerNumberSearchViewModel Model { get; init; }

    public PupilPremiumSearchByUniquePupilNumberResponse Response { get; init; }

    public PupilPremiumLearnerNumericSearchMappingContext(
        LearnerNumberSearchViewModel model,
        PupilPremiumSearchByUniquePupilNumberResponse response)
    {
        ArgumentNullException.ThrowIfNull(model);
        Model = model;

        ArgumentNullException.ThrowIfNull(response);
        Response = response;
    }

    public static PupilPremiumLearnerNumericSearchMappingContext Create(
        LearnerNumberSearchViewModel model,
        PupilPremiumSearchByUniquePupilNumberResponse response) =>
        new(model, response);
}
