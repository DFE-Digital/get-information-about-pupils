using DfE.GIAP.Core.Common.Domain;
using DfE.GIAP.Core.MyPupils.Domain.Authorisation;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.Services;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Domain.Aggregate;
public sealed class UserAggregateRoot : AggregateRoot<UserId>
{
    private const int PUPIL_LIST_LIMIT = 4000;
    private IEnumerable<PupilIdentifier> _pupilIds;
    private readonly IAggregatePupilsForMyPupilsDomainService _aggregatePupilsForMyPupilsDomainService;

    public UserAggregateRoot(
        UserId identifier,
        IEnumerable<PupilIdentifier> pupilIds,
        IAggregatePupilsForMyPupilsDomainService aggregatePupilsForMyPupilsDomainService) : base(identifier)
    {
        _pupilIds = pupilIds.DistinctBy((pupilId) => pupilId.UniquePupilNumber)
                .Take(PUPIL_LIST_LIMIT);
        _aggregatePupilsForMyPupilsDomainService = aggregatePupilsForMyPupilsDomainService;
    }

    public async Task<IEnumerable<PupilDto>> GetMyPupils(
        PupilAuthorisationContext authorisationContext,
        PupilSelectionDomainCriteria pupilSelectionCriteria)
    {
        IEnumerable<Pupil> pupils =
            await _aggregatePupilsForMyPupilsDomainService.GetPupilsAsync(
                _pupilIds,
                authorisationContext,
                pupilSelectionCriteria);

        return pupils.Select((pupil) => new PupilDto(pupil));
    }

    public void RemovePupils(IEnumerable<PupilId> pupilsToRemove) // TODO future consider raising domain event
    {

        HashSet<PupilId> removePupilIds = pupilsToRemove.ToHashSet();
        Func<PupilId, bool> ShouldRemovePupil = removePupilIds.Contains;

        if (!_pupilIds.Any(pupilIdentifier => ShouldRemovePupil(pupilIdentifier.PupilId)))
        {
            throw new InvalidOperationException("None of the pupils exist in the list.");
        }

        _pupilIds =
            _pupilIds
            .Where((pupilIdentifier) => !ShouldRemovePupil(pupilIdentifier.PupilId))
            .ToList();

    }

    public IReadOnlyCollection<PupilId> GetUpdatedPupilIds() => _pupilIds.Select(t => t.PupilId).ToList().AsReadOnly();
}
