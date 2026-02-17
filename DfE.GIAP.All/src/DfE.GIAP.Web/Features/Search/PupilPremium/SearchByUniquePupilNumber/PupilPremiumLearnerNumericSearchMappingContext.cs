using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;
using DfE.GIAP.Web.ViewModels.Search;

namespace DfE.GIAP.Web.Features.Search.PupilPremium.SearchByUniquePupilNumber;

public record PupilPremiumLearnerNumericSearchMappingContext
{
    public LearnerNumberSearchViewModel Model { get; init; }

    public SearchResponse<PupilPremiumLearners> Response { get; init; }

    public PupilPremiumLearnerNumericSearchMappingContext(
        LearnerNumberSearchViewModel model,
        SearchResponse<PupilPremiumLearners> response)
    {
        ArgumentNullException.ThrowIfNull(model);
        Model = model;

        ArgumentNullException.ThrowIfNull(response);
        Response = response;
    }

    public static PupilPremiumLearnerNumericSearchMappingContext Create(
        LearnerNumberSearchViewModel model,
        SearchResponse<PupilPremiumLearners> response) =>
        new(model, response);
}
