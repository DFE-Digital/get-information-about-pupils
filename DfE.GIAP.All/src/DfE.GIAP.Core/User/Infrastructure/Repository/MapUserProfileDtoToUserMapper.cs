using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Domain.Aggregate;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.User.Infrastructure.Repository;
public sealed class MapUserProfileDtoToUserMapper : IMapper<UserProfileDto, Application.Repository.UserReadRepository.User>
{
    public Application.Repository.UserReadRepository.User Map(UserProfileDto dto)
    {
        IEnumerable<PupilItemDto> myPupils = dto.MyPupils ?? [];
        
        UserId id = new(dto.UserId!);

        IEnumerable<PupilIdentifier> pupilIdentifiers
            = myPupils
                .Where(
                    (myPupil) => TryCreateUpn(myPupil.PupilId) is not null)
                .Select(
                    (myPupil) => new PupilIdentifier(
                        PupilId: new PupilId(myPupil.Id),
                        UniquePupilNumber: new UniquePupilNumber(myPupil.PupilId)));

        return new Application.Repository.UserReadRepository.User(id, pupilIdentifiers);
    }

    private static UniquePupilNumber? TryCreateUpn(string id) =>
        UniquePupilNumber.TryCreate(id, out UniquePupilNumber? upn) ? upn : null;

}
