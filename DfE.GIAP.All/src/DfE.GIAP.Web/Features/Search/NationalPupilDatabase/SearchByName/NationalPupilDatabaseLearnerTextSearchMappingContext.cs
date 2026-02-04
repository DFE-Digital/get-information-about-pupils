using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase;
using DfE.GIAP.Web.ViewModels.Search;

namespace DfE.GIAP.Web.Features.Search.NationalPupilDatabase.SearchByName;

public record NationalPupilDatabaseLearnerTextSearchMappingContext
{
    public LearnerTextSearchViewModel Model { get; init; }

    public NationalPupilDatabaseSearchResponse Response { get; init; }

    public NationalPupilDatabaseLearnerTextSearchMappingContext(
        LearnerTextSearchViewModel model,
        NationalPupilDatabaseSearchResponse response)
    {
        ArgumentNullException.ThrowIfNull(model);
        Model = model;

        ArgumentNullException.ThrowIfNull(response);
        Response = response;
    }

    public static NationalPupilDatabaseLearnerTextSearchMappingContext Create(
        LearnerTextSearchViewModel model,
        NationalPupilDatabaseSearchResponse response) => new(model, response);
}
