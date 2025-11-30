using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Users.Application.Models;
using DfE.GIAP.Core.Users.Infrastructure.Repositories.Dtos;

namespace DfE.GIAP.Core.Users.Infrastructure.Repositories.Mappers;
public sealed class UserDtoToUserMapper : IMapper<UserDto, User>
{
    public User Map(UserDto dto)
    {
        UserId id = new(dto.id);

        return new User(
            id,
            dto.LastLoggedIn);
    }
}
