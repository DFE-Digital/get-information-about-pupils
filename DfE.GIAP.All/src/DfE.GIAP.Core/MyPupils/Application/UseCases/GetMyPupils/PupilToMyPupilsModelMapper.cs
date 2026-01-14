using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
internal sealed class PupilToMyPupilsModelMapper : IMapper<Pupil, MyPupilsModel>
{
    public MyPupilsModel Map(Pupil pupil)
    {
        ArgumentNullException.ThrowIfNull(pupil);
        return new()
        {
            UniquePupilNumber = pupil.Identifier.Value,
            DateOfBirth = pupil.DateOfBirth?.ToString() ?? string.Empty,
            Forename = pupil.Forename,
            Surname = pupil.Surname,
            Sex = pupil.Sex,
            IsPupilPremium = pupil.IsOfPupilType(PupilType.PupilPremium),
            LocalAuthorityCode = pupil.LocalAuthorityCode,
        };
    }
}
