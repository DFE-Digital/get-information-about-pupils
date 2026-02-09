using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.SearchByUniqueLearnerNumber;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Web.ViewModels.Search;

namespace DfE.GIAP.Web.Features.Search.FurtherEducation.SearchByUniqueLearnerNumber;

internal sealed class FurtherEducationLearnerNumericSearchResponseToViewModelMapper :
    IMapper<FurtherEducationLearnerNumericSearchMappingContext, LearnerNumberSearchViewModel>
{
    // Mapper for converting individual FurtherEducationLearner domain entities into UI-facing Learner view models.
    private readonly IMapper<FurtherEducationLearner, Learner> _furtherEducationLearnerToViewModelMapper;

    public FurtherEducationLearnerNumericSearchResponseToViewModelMapper(
        IMapper<FurtherEducationLearner, Learner> furtherEducationLearnerToViewModelMapper)
    {
        ArgumentNullException.ThrowIfNull(furtherEducationLearnerToViewModelMapper);
        _furtherEducationLearnerToViewModelMapper = furtherEducationLearnerToViewModelMapper;
    }

    public LearnerNumberSearchViewModel Map(FurtherEducationLearnerNumericSearchMappingContext input)
    {
        // Map each learner from domain to view model using the injected learner mapper.
        List<Learner> learners =
            input.Response.LearnerSearchResults?.Learners
                .Select(_furtherEducationLearnerToViewModelMapper.Map)
                .ToList() ?? [];

        input.Model.Learners = learners;

        // Populate meta-data fields for pagination and UI messaging.
        input.Model.Total = input.Response.TotalNumberOfResults;

        return input.Model;
    }
}

public sealed class FurtherEducationLearnerNumericSearchMappingContext
{
    public LearnerNumberSearchViewModel Model { get; init; }

    public FurtherEducationSearchByUniqueLearnerNumberResponse Response { get; init; }

    public FurtherEducationLearnerNumericSearchMappingContext(
        LearnerNumberSearchViewModel model,
        FurtherEducationSearchByUniqueLearnerNumberResponse response)
    {
        Model = model ?? throw new ArgumentNullException(nameof(model));
        Response = response ?? throw new ArgumentNullException(nameof(response));
    }

    public static FurtherEducationLearnerNumericSearchMappingContext Create(
        LearnerNumberSearchViewModel model,
        FurtherEducationSearchByUniqueLearnerNumberResponse response) =>
        new(model, response);
}
