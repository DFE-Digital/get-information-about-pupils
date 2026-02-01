using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium;
using DfE.GIAP.Web.ViewModels.Search;

namespace DfE.GIAP.Web.Features.Search.PupilPremium.SearchByName;

public record PupilPremiumLearnerTextSearchMappingContext
{

    public LearnerTextSearchViewModel Model { get; init; }

    public PupilPremiumSearchResponse Response { get; init; }

    public PupilPremiumLearnerTextSearchMappingContext(
        LearnerTextSearchViewModel model,
        PupilPremiumSearchResponse response)
    {
        ArgumentNullException.ThrowIfNull(model);
        Model = model;

        ArgumentNullException.ThrowIfNull(response);
        Response = response;
    }

    public static PupilPremiumLearnerTextSearchMappingContext Create(
        LearnerTextSearchViewModel model,
        PupilPremiumSearchResponse response) => new(model, response);
}
