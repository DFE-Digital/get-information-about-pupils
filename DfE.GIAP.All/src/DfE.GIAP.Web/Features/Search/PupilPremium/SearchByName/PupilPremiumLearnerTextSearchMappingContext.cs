using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.SearchByName;
using DfE.GIAP.Web.ViewModels.Search;

namespace DfE.GIAP.Web.Features.Search.PupilPremium.SearchByName;

public record PupilPremiumLearnerTextSearchMappingContext
{
    public LearnerTextSearchViewModel Model { get; init; }

    public PupilPremiumSearchByNameResponse Response { get; init; }

    public PupilPremiumLearnerTextSearchMappingContext(
        LearnerTextSearchViewModel model,
        PupilPremiumSearchByNameResponse response)
    {
        ArgumentNullException.ThrowIfNull(model);
        Model = model;

        ArgumentNullException.ThrowIfNull(response);
        Response = response;
    }

    public static PupilPremiumLearnerTextSearchMappingContext Create(
        LearnerTextSearchViewModel model,
        PupilPremiumSearchByNameResponse response) => new(model, response);
}
