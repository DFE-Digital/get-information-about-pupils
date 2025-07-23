using DfE.GIAP.Core.Common.Domain;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Domain.Aggregate;
public sealed class UserAggregateRoot : AggregateRoot<UserId>
{
    private const int PUPIL_LIST_LIMIT = 4000;
    private readonly List<Pupil> _myPupils;

    public UserAggregateRoot(
        UserId identifier,
        IEnumerable<Pupil>? myPupils) : base(identifier)
    {
        _myPupils = myPupils?
            .Take(PUPIL_LIST_LIMIT)
            .ToList() ?? [];
    }

    public IEnumerable<PupilDto> GetMyPupils() 
        => _myPupils.Distinct()
                .Select((pupil)
                    => new PupilDto(pupil));

    public bool HasPupil(PupilId id) => _myPupils.Any(p => p.Identifier == id);


    // TODO consider actions to delete will need identifiers attached to each item (what is it currently doing?)
    public void AddPupilToMyPupils(PupilDto pupil) // TODO specific write model
    {
        if (_myPupils.Any(myPupil => myPupil.Equals(pupil)))
        {
            throw new InvalidOperationException("Pupil already added.");
        }
        //_myPupils.Add(pupil);
    }
}
