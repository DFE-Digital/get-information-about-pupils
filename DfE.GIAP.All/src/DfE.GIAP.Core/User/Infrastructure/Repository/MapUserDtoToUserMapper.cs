using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.Extensions;
using DfE.GIAP.Core.User.Application;
using DfE.GIAP.Core.User.Infrastructure.Repository.Dtos;

namespace DfE.GIAP.Core.User.Infrastructure.Repository;
public sealed class MapUserProfileDtoToUserMapper : IMapper<UserDto, Application.User>
{
    public Application.User Map(UserDto dto)
    {
        IEnumerable<MyPupilsItemDto> myPupils = dto.MyPupils?.Pupils ?? [];

        UserId id = new(dto.id!);

        return new Application.User(
            id,
            myPupils.Select(t => t.UPN).ToUniquePupilNumbers());
    }
}
