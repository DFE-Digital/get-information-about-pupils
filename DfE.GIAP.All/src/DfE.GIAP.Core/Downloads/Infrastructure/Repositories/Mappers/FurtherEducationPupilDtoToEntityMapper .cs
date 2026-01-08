using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Infrastructure.Repositories.DataTransferObjects;

namespace DfE.GIAP.Core.Downloads.Infrastructure.Repositories.Mappers;

/// <summary>
/// Maps a FurtherEducationPupilDto object to a FurtherEducationPupil entity.
/// </summary>
internal class FurtherEducationPupilDtoToEntityMapper : IMapper<FurtherEducationPupilDto, FurtherEducationPupil>
{
    /// <summary>
    /// Converts a FurtherEducationPupilDto into a FurtherEducationPupil entity.
    /// </summary>
    /// <param name="input">The DTO containing raw pupil data.</param>
    /// <returns>A FurtherEducationPupil entity populated with the DTO values.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the input DTO is null.</exception>
    public FurtherEducationPupil Map(FurtherEducationPupilDto input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return new FurtherEducationPupil
        {
            UniqueLearnerNumber = input.UniqueLearnerNumber,
            Forename = input.Forename,
            Surname = input.Surname,
            Gender = input.Gender,
            DOB = input.DOB,
            ConcatenatedName = input.ConcatenatedName,
            PupilPremium = input.PupilPremium?.Select(dto => new PupilPremiumEntry
            {
                NationalCurriculumYear = dto.NationalCurriculumYear,
                FullTimeEquivalent = dto.FullTimeEquivalent,
                AcademicYear = dto.AcademicYear
            }).ToList() ?? [],
            specialEducationalNeeds = input.specialEducationalNeeds?.Select(dto => new SpecialEducationalNeedsEntry
            {
                NationalCurriculumYear = dto.NationalCurriculumYear,
                Provision = dto.Provision,
                AcademicYear = dto.AcademicYear
            }).ToList() ?? []
        };
    }
}
