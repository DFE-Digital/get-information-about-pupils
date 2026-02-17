using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.SearchByName;
using DfE.GIAP.Web.ViewModels.Search;

namespace DfE.GIAP.Web.Features.Search.NationalPupilDatabase.SearchByName;

public record NationalPupilDatabaseLearnerTextSearchMappingContext
{
    public LearnerTextSearchViewModel Model { get; init; }

    public SearchResponse<NationalPupilDatabaseLearners> Response { get; init; }

    public NationalPupilDatabaseLearnerTextSearchMappingContext(
        LearnerTextSearchViewModel model,
        SearchResponse<NationalPupilDatabaseLearners> response)
    {
        ArgumentNullException.ThrowIfNull(model);
        Model = model;

        ArgumentNullException.ThrowIfNull(response);
        Response = response;
    }

    public static NationalPupilDatabaseLearnerTextSearchMappingContext Create(
        LearnerTextSearchViewModel model,
        SearchResponse<NationalPupilDatabaseLearners> response) => new(model, response);
}
