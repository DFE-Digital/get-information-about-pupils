using DfE.GIAP.Core.Search.Application.Models.Learner.PupilPremium;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Web.ViewModels.Search;

namespace DfE.GIAP.Web.Features.Search.PupilPremium;

public sealed class PupilPremiumLearnerNumericSearchResponseToViewModelMapper :
    IMapper<PupilPremiumLearnerNumericSearchMappingContext, LearnerNumberSearchViewModel>
{
    private readonly IMapper<PupilPremiumLearner, Learner> _pupilPremiumModelToLearnerMapper;

    public PupilPremiumLearnerNumericSearchResponseToViewModelMapper(
        IMapper<PupilPremiumLearner, Learner> pupilPremiumModelToLearnerMapper)
    {
        ArgumentNullException.ThrowIfNull(pupilPremiumModelToLearnerMapper);
        _pupilPremiumModelToLearnerMapper = pupilPremiumModelToLearnerMapper;
    }

    public LearnerNumberSearchViewModel Map(PupilPremiumLearnerNumericSearchMappingContext input)
    {
        List<Learner> learners =
            input.Response.LearnerSearchResults?.Values
                .Select(_pupilPremiumModelToLearnerMapper.Map)
                .ToList() ?? [];

        input.Model.Learners = learners;

        // Populate meta-data fields for pagination and UI messaging.
        input.Model.Total = input.Response.TotalNumberOfResults.Count;

        return input.Model;
    }
}

public record PupilPremiumLearnerNumericSearchMappingContext
{

    public LearnerNumberSearchViewModel Model { get; init; }

    public PupilPremiumSearchResponse Response { get; init; }

    public PupilPremiumLearnerNumericSearchMappingContext(
        LearnerNumberSearchViewModel model,
        PupilPremiumSearchResponse response)
    {
        ArgumentNullException.ThrowIfNull(model);
        Model = model;

        ArgumentNullException.ThrowIfNull(response);
        Response = response;
    }

    public static PupilPremiumLearnerNumericSearchMappingContext Create(
        LearnerNumberSearchViewModel model,
        PupilPremiumSearchResponse response) =>
        new(model, response);
}
