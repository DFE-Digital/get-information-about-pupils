using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.SearchByName;
using DfE.GIAP.Web.ViewModels.Search;

namespace DfE.GIAP.Web.Features.Search.NationalPupilDatabase.SearchByName;

public record NationalPupilDatabaseLearnerTextSearchMappingContext
{
    public LearnerTextSearchViewModel Model { get; init; }

    public NationalPupilDatabaseSearchByNameResponse Response { get; init; }

    public NationalPupilDatabaseLearnerTextSearchMappingContext(
        LearnerTextSearchViewModel model,
        NationalPupilDatabaseSearchByNameResponse response)
    {
        ArgumentNullException.ThrowIfNull(model);
        Model = model;

        ArgumentNullException.ThrowIfNull(response);
        Response = response;
    }

    public static NationalPupilDatabaseLearnerTextSearchMappingContext Create(
        LearnerTextSearchViewModel model,
        NationalPupilDatabaseSearchByNameResponse response) => new(model, response);
}
