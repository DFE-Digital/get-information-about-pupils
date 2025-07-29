using DfE.GIAP.Core.Common.Domain;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Domain.Aggregate;
public sealed class UserAggregateRoot : AggregateRoot<UserId>
{
    private const int PUPIL_LIST_LIMIT = 4000;
    private IEnumerable<Pupil> _pupils;

    public UserAggregateRoot(
        UserId identifier,
        IEnumerable<Pupil> pupils) : base(identifier)
    {
        _pupils =
            pupils.DistinctBy((pupilId) => pupilId.Identifier)
                .Take(PUPIL_LIST_LIMIT);
    }

    public void RemovePupils(IEnumerable<UniquePupilNumber> pupilsToRemove) // TODO future consider raising domain event
    {
        Func<UniquePupilNumber, bool> RemovePupil = pupilsToRemove.Contains;

        if (!_pupils.Select(t => t.Identifier).Any(RemovePupil))
        {
            throw new InvalidOperationException($"None of the identifiers exist in MyPupils: {string.Join(',', pupilsToRemove.Select(t => t.Value))}");
        }

        _pupils = _pupils.Where(t => !RemovePupil(t.Identifier));
    }

    public IReadOnlyCollection<UniquePupilNumber> GetUpdatedPupilIds() => _pupils.Select(t => t.Identifier).ToList().AsReadOnly();
}
