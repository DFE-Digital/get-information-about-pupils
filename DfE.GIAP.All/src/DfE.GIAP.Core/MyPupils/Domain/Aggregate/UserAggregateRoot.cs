using DfE.GIAP.Core.Common.Domain;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Domain.Aggregate;
public sealed class UserAggregateRoot : AggregateRoot<UserId>
{
    private readonly List<Pupil> _myPupils;

    public UserAggregateRoot(
        UserId identifier,
        IEnumerable<Pupil>? myPupils) : base(identifier)
    {
        _myPupils = myPupils?.ToList() ?? [];
    }

    public IEnumerable<Pupil> GetMyPupils()
        => _myPupils.Distinct();

    // TODO consider actions to delete will need identifiers attached to each item (what is it currently doing?)
    public void AddPupilToMyPupils(Pupil pupil)
    {
        if (_myPupils.Any(myPupil => myPupil.Equals(pupil)))
        {
            throw new InvalidOperationException("Pupil already added.");
        }
        _myPupils.Add(pupil);
    }
}
