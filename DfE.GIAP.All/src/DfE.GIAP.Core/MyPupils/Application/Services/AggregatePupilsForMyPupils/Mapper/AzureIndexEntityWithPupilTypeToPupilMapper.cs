using DfE.GIAP.Core.Common.Domain;
using DfE.GIAP.Core.Common.ValueObjects;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.DataTransferObjects;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.Mapper;
internal sealed class AzureIndexEntityWithPupilTypeToPupilMapper : IMapper<AzureIndexEntityWithPupilType, Pupil>
{
    public Pupil Map(AzureIndexEntityWithPupilType input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return new(
            identifier: new UniquePupilNumber(input.SearchIndexDto.UPN),
            pupilType: input.PupilType,
            name: new PupilName(input.SearchIndexDto.Forename, input.SearchIndexDto.Surname),
            dateOfBirth: input.SearchIndexDto.DOB,
            sex: new Sex(input.SearchIndexDto.Sex),
            localAuthorityCode: int.TryParse(input.SearchIndexDto.LocalAuthority, out int code) ? new LocalAuthorityCode(code) : null
        );
    }
}
