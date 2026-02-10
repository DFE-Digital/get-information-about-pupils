using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.SearchByUniquePupilNumber;
using DfE.GIAP.Web.ViewModels.Search;

namespace DfE.GIAP.Web.Features.Search.NationalPupilDatabase.SearchByUniquePupilNumber;

public record NationalPupilDatabaseLearnerNumericSearchMappingContext
{
    public NationalPupilDatabaseLearnerNumericSearchMappingContext(
        LearnerNumberSearchViewModel model,
        NationalPupilDatabaseSearchByUniquePupilNumberResponse response)
    {
        ArgumentNullException.ThrowIfNull(model);
        Model = model;

        ArgumentNullException.ThrowIfNull(response);
        Response = response;
    }

    public LearnerNumberSearchViewModel Model { get; }
    public NationalPupilDatabaseSearchByUniquePupilNumberResponse Response { get; }

    public static NationalPupilDatabaseLearnerNumericSearchMappingContext Create(
        LearnerNumberSearchViewModel model,
        NationalPupilDatabaseSearchByUniquePupilNumberResponse response) => new(model, response);
}
