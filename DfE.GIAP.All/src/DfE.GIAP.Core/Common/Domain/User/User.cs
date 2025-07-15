using DfE.GIAP.Core.Common.Domain.Contracts;
using DfE.GIAP.Core.MyPupils.Domain;

namespace DfE.GIAP.Core.Common.Domain.User;
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


