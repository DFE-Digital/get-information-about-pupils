using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.Extensions;
using DfE.GIAP.Core.Users.Application;
using DfE.GIAP.Core.Users.Infrastructure.Repository.Dtos;

namespace DfE.GIAP.Core.Users.Infrastructure.Repository;
public sealed class UserDtoToUserMapper : IMapper<UserDto, User>
{
    public User Map(UserDto dto)
    {
        IEnumerable<MyPupilsItemDto> myPupils = dto.MyPupils?.Pupils ?? [];

        UserId id = new(dto.id!);

        return new User(
            id,
            myPupils.Select(t => t.UPN).ToUniquePupilNumbers(),
            dto.LastLoggedIn);
    }
}
