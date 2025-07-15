using DfE.GIAP.Core.MyPupils.Domain.Entities;

namespace DfE.GIAP.Core.Common.Domain.User;
public sealed class UserAggregateRoot : AggregateRoot<UserIdentifier>
{
    private readonly List<MyPupil> _myPupils;

    public UserAggregateRoot(
        UserIdentifier identifier,
        IEnumerable<MyPupil>? myPupils) : base(identifier)
    {
        _myPupils = myPupils?.ToList() ?? [];
    }

    public IEnumerable<MyPupil> GetMyPupils() => _myPupils;

    // TODO consider actions to delete will need identifiers attached to each item (what is it currently doing?)
    public void AddPupil(MyPupil pupil)
    {
        if (_myPupils.Any(myPupil => myPupil.Equals(pupil)))
        {
            throw new InvalidOperationException("Pupil already added.");
        }
        _myPupils.Add(pupil);
    }
}


