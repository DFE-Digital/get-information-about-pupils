using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;
using DfE.GIAP.Web.ViewModels.Search;

namespace DfE.GIAP.Web.Features.Search.FurtherEducation.SearchByUniqueLearnerNumber;

public sealed class FurtherEducationLearnerNumericSearchMappingContext
{
    public LearnerNumberSearchViewModel Model { get; init; }

    public SearchResponse<FurtherEducationLearners> Response { get; init; }

    public FurtherEducationLearnerNumericSearchMappingContext(
        LearnerNumberSearchViewModel model,
        SearchResponse<FurtherEducationLearners> response)
    {
        Model = model ?? throw new ArgumentNullException(nameof(model));
        Response = response ?? throw new ArgumentNullException(nameof(response));
    }

    public static FurtherEducationLearnerNumericSearchMappingContext Create(
        LearnerNumberSearchViewModel model,
        SearchResponse<FurtherEducationLearners> response) =>
        new(model, response);
}
