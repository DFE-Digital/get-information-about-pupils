using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium;
using DfE.GIAP.Web.ViewModels.Search;

namespace DfE.GIAP.Web.Features.Search.PupilPremium.SearchByUniquePupilNumber;

public record PupilPremiumLearnerNumericSearchMappingContext
{

    public LearnerNumberSearchViewModel Model { get; init; }

    public PupilPremiumSearchResponse Response { get; init; }

    public PupilPremiumLearnerNumericSearchMappingContext(
        LearnerNumberSearchViewModel model,
        PupilPremiumSearchResponse response)
    {
        ArgumentNullException.ThrowIfNull(model);
        Model = model;

        ArgumentNullException.ThrowIfNull(response);
        Response = response;
    }

    public static PupilPremiumLearnerNumericSearchMappingContext Create(
        LearnerNumberSearchViewModel model,
        PupilPremiumSearchResponse response) =>
        new(model, response);
}
