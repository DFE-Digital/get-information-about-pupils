using DfE.GIAP.Core.Common.Domain;
using DfE.GIAP.Core.Users.Application.Models;

namespace DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
public sealed class MyPupilsId : ValueObject<MyPupilsId>
{
    public MyPupilsId(UserId userId)
    {
        ArgumentNullException.ThrowIfNull(userId);
        Value = userId.Value;
    }

    public string Value { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}