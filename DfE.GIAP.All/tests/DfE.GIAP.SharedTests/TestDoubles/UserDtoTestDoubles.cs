using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.Core.Users.Application;
using DfE.GIAP.Core.Users.Infrastructure.Repositories.DataTransferObjects;

namespace DfE.GIAP.SharedTests.TestDoubles;

public static class UserDtoTestDoubles
{
    public static UserDto Default() => new() { id = Guid.NewGuid().ToString() };

    public static UserDto Create(UserId id) => new() { id = id.Value };
}
