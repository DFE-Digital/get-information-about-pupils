using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.Extensions;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.User.Infrastructure.Repository;
public sealed class MapUserProfileDtoToUserMapper : IMapper<UserDto, Application.Repository.UserReadRepository.User>
{
    public Application.Repository.UserReadRepository.User Map(UserDto dto)
    {
        IEnumerable<MyPupilItemDto> myPupils = dto.MyPupils?.Pupils ?? [];

        UserId id = new(dto.id!);

        return new Application.Repository.UserReadRepository.User(
            id,
            myPupils.Select(t => t.UPN).ToUniquePupilNumbers());
    }
}
