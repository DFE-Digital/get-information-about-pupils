using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Web.ViewModels.Search;

namespace DfE.GIAP.Web.Features.Search.PupilPremium.SearchByUniquePupilNumber;

public sealed class PupilPremiumLearnerNumericSearchMappingContextToViewModelMapper :
    IMapper<PupilPremiumLearnerNumericSearchMappingContext, LearnerNumberSearchViewModel>
{
    private readonly IMapper<PupilPremiumLearner, Learner> _pupilPremiumModelToLearnerMapper;

    public PupilPremiumLearnerNumericSearchMappingContextToViewModelMapper(
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
        input.Model.Total = input.Response.TotalNumberOfResults;

        return input.Model;
    }
}
