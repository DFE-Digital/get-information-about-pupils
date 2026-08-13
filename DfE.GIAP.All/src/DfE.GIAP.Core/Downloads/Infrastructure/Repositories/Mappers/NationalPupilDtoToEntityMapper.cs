using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.Entries;
using DfE.GIAP.Core.Downloads.Infrastructure.DataTransferObjects;
using DfE.GIAP.Core.Downloads.Infrastructure.Repositories.Mappers.Resolvers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DfE.GIAP.Core.Downloads.Infrastructure.Repositories.Mappers;

/// <summary>
/// Maps a NationalPupilDto object to a NationalPupil entity.
/// </summary>
internal class NationalPupilDtoToEntityMapper : IMapper<NationalPupilDto, NationalPupil>
{
    /// <summary>
    /// Converts a NationalPupilDto into a NationalPupil entity.
    /// </summary>
    /// <param name="input">The DTO containing raw pupil data.</param>
    /// <returns>A NationalPupil entity populated with the DTO values.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the input DTO is null.</exception>
    public NationalPupil Map(NationalPupilDto input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return new NationalPupil
        {
            Upn = input.Upn,
            Id = input.Id,
            PupilMatchingRef = input.PupilMatchingRef,
            LA = input.LA,
            Estab = input.Estab,
            Urn = input.Urn,
            Surname = input.Surname,
            Forename = input.Forename,
            MiddleName = input.MiddleName,
            Gender = input.Gender,
            Sex = input.Sex,
            DOB = input.DOB,
            MTC = input.MTC?.Select(dto => new MtcEntry
            {
                ACADYR = dto.ACADYR,
                PupilMatchingRef = dto.PupilMatchingRef,
                UPN = dto.UPN,
                Surname = dto.Surname,
                Forename = dto.Forename,
                Sex = dto.Sex,
                DOB = dto.DOB,
                LA = dto.LA,
                LA_9Code = dto.LA_9Code,
                Estab = dto.Estab,
                LAEstab = dto.LAEstab,
                URN = dto.URN,
                ToECode = dto.ToECode,
                FormMark = dto.FormMark,
                PupilStatus = dto.PupilStatus,
                ReasonNotTakingCheck = dto.ReasonNotTakingCheck
            }).ToList() ?? [],
        };
    }
}
