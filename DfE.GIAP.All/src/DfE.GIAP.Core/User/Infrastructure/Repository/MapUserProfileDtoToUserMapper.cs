using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Domain.Aggregate;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.User.Infrastructure.Repository;
public sealed class MapUserProfileDtoToUserMapper : IMapper<UserDto, Application.Repository.UserReadRepository.User>
{
    public Application.Repository.UserReadRepository.User Map(UserDto dto)
    {
        IEnumerable<MyPupilItemDto> myPupils = dto.MyPupils ?? [];
        
        UserId id = new(dto.id!);

        IEnumerable<PupilIdentifier> pupilIdentifiers
            = myPupils
                .Where(
                    (myPupil) => TryCreateUpn(myPupil.UPN) is not null)
                .Select(
                    (myPupil) => new PupilIdentifier(
                        PupilId: new PupilId(myPupil.Id),
                        UniquePupilNumber: new UniquePupilNumber(myPupil.UPN)));

        return new Application.Repository.UserReadRepository.User(id, pupilIdentifiers);
    }

    private static UniquePupilNumber? TryCreateUpn(string id) =>
        UniquePupilNumber.TryCreate(id, out UniquePupilNumber? upn) ? upn : null;

}
