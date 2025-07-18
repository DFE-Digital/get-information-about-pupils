using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.User.Infrastructure;
public sealed class MapUserProfileDtoToUserMapper : IMapper<UserProfileDto, Application.Repository.User>
{
    public Application.Repository.User Map(UserProfileDto dto)
    {
        IEnumerable<UniquePupilNumber> upns =
            (dto.MyPupilList ?? Enumerable.Empty<PupilItemDto>())
            .Select(t => t.PupilId)
            .Concat(dto.PupilList ?? Enumerable.Empty<string>()) // TODO understand why this is joined, does PupilList even exist in persistence
            .Select(TryCreateUpn!)
            .Where(t => t is not null)!;


        UserId id = new(dto.UserId!);

        return new Application.Repository.User(id, upns);
    }

    private static UniquePupilNumber? TryCreateUpn(string id) =>
        UniquePupilNumber.TryCreate(id, out UniquePupilNumber? upn) ? upn : null;

}
