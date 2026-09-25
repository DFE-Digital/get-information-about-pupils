using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;
using DfE.GIAP.Web.ViewModels.Search;

namespace DfE.GIAP.Web.Features.Search.PupilPremium.SearchByName;

public record PupilPremiumLearnerTextSearchMappingContext
{
    public LearnerTextSearchViewModel Model { get; init; }

    public SearchResponse<PupilPremiumLearners> Response { get; init; }

    public PupilPremiumLearnerTextSearchMappingContext(
        LearnerTextSearchViewModel model,
        SearchResponse<PupilPremiumLearners> response)
    {
        ArgumentNullException.ThrowIfNull(model);
        Model = model;

        ArgumentNullException.ThrowIfNull(response);
        Response = response;
    }

    public static PupilPremiumLearnerTextSearchMappingContext Create(
        LearnerTextSearchViewModel model,
        SearchResponse<PupilPremiumLearners> response) => new(model, response);
}
