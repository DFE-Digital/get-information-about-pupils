using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Pupil.Domain;
using DfE.GIAP.Core.User.Domain.Aggregate;

namespace DfE.GIAP.Core.User.Infrastructure.Repository;
public sealed class UserProfileDtoToUserMapper : IMapper<UserProfileDto, Application.Repository.User>
{
    public Application.Repository.User Map(UserProfileDto dto)
    {
        IEnumerable<UniquePupilIdentifier> upns =
            dto.MyPupilList
            .Select(t => t.PupilId!)
            .Where(t => !string.IsNullOrEmpty(t))
            .Concat(dto.PupilList ?? []) // TODO understand why this is joined, does PupilList even exist in persistence
            .Distinct()
            .Select(upn => new UniquePupilIdentifier(upn));// TODO should we be making these distinct, is Web doing this currently

        UserIdentifier id = new(dto.UserId!);

        return new Application.Repository.User(id, upns);
    }
}
