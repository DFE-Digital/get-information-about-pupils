using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase;
using DfE.GIAP.Web.ViewModels.Search;

namespace DfE.GIAP.Web.Features.Search.NationalPupilDatabase.SearchByUniquePupilNumber;

public record NationalPupilDatabaseLearnerNumericSearchMappingContext
{
    public NationalPupilDatabaseLearnerNumericSearchMappingContext(
        LearnerNumberSearchViewModel model,
        NationalPupilDatabaseSearchResponse response)
    {
        ArgumentNullException.ThrowIfNull(model);
        Model = model;

        ArgumentNullException.ThrowIfNull(response);
        Response = response;
    }

    public LearnerNumberSearchViewModel Model { get; }
    public NationalPupilDatabaseSearchResponse Response { get; }

    public static NationalPupilDatabaseLearnerNumericSearchMappingContext Create(
        LearnerNumberSearchViewModel model,
        NationalPupilDatabaseSearchResponse response) => new(model, response);
}
