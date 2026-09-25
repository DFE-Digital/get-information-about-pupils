using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;
using DfE.GIAP.Web.Features.Search.LegacyModels.Learner;
using DfE.GIAP.Web.ViewModels.Search;

namespace DfE.GIAP.Web.Features.Search.NationalPupilDatabase.SearchByUniquePupilNumber;

internal sealed class NationalPupilDatabaseLearnerNumericSearchMappingContextToViewModelMapper
    : IMapper<NationalPupilDatabaseLearnerNumericSearchMappingContext, LearnerNumberSearchViewModel>
{
    private readonly IMapper<NationalPupilDatabaseLearner, Learner> _pupilPremiumModelToLearnerMapper;

    public NationalPupilDatabaseLearnerNumericSearchMappingContextToViewModelMapper(
        IMapper<NationalPupilDatabaseLearner, Learner> pupilPremiumModelToLearnerMapper)
    {
        ArgumentNullException.ThrowIfNull(pupilPremiumModelToLearnerMapper);
        _pupilPremiumModelToLearnerMapper = pupilPremiumModelToLearnerMapper;
    }

    public LearnerNumberSearchViewModel Map(NationalPupilDatabaseLearnerNumericSearchMappingContext input)
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
