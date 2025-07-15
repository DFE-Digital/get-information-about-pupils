using DfE.GIAP.Core.Common.Domain;

namespace DfE.GIAP.Core.MyPupils.Domain;
public sealed class User : AggregateRoot<UserId>
{
    private readonly IEnumerable<MyPupil> _myPupils;

    public User(
        UserId identifier,
        IEnumerable<MyPupil>? myPupils) : base(identifier)
    {
        _myPupils = myPupils ?? [];
    }

    public IEnumerable<MyPupil> GetMyPupils() => _myPupils;
}


