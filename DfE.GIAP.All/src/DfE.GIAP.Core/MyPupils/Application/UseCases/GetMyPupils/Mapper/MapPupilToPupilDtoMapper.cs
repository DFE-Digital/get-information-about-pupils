using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Mapper;
internal sealed class MapPupilToPupilDtoMapper : IMapper<Pupil, PupilDto>
{
    public PupilDto Map(Pupil pupil)
    {
        return new PupilDto()
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
