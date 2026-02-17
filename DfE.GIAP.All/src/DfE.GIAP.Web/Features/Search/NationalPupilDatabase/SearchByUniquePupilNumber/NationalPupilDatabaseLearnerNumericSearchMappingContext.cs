using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;
using DfE.GIAP.Web.ViewModels.Search;

namespace DfE.GIAP.Web.Features.Search.NationalPupilDatabase.SearchByUniquePupilNumber;

public record NationalPupilDatabaseLearnerNumericSearchMappingContext
{
    public NationalPupilDatabaseLearnerNumericSearchMappingContext(
        LearnerNumberSearchViewModel model,
        SearchResponse<NationalPupilDatabaseLearners> response)
    {
        ArgumentNullException.ThrowIfNull(model);
        Model = model;

        ArgumentNullException.ThrowIfNull(response);
        Response = response;
    }

    public LearnerNumberSearchViewModel Model { get; }
    public SearchResponse<NationalPupilDatabaseLearners> Response { get; }

    public static NationalPupilDatabaseLearnerNumericSearchMappingContext Create(
        LearnerNumberSearchViewModel model,
        SearchResponse<NationalPupilDatabaseLearners> response) => new(model, response);
}
