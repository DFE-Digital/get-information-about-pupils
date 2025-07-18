using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Repository;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Infrastructure.Repository;
public sealed class MapUserProfileDtoToUserMapper : IMapper<UserProfileDto, User>
{
    public User Map(UserProfileDto dto)
    {
        IEnumerable<UniquePupilNumber> upns =
            (dto.MyPupilList ?? Enumerable.Empty<PupilItemDto>())
            .Select(t => t.PupilId)
            .Concat(dto.PupilList ?? Enumerable.Empty<string>()) // TODO understand why this is joined, does PupilList even exist in persistence
            .Select(TryCreateUpn!)
            .Where(t => t is not null)!;


        UserId id = new(dto.UserId!);

        return new User(id, upns);
    }

    private static UniquePupilNumber? TryCreateUpn(string id) =>
        UniquePupilNumber.TryCreate(id, out UniquePupilNumber? upn) ? upn : null;

}
