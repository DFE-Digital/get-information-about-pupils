using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;
using DfE.GIAP.Core.Search.Infrastructure.NationalPupilDatabase.DataTransferObjects;

namespace DfE.GIAP.Core.Search.Infrastructure.NationalPupilDatabase.Mappers;
internal sealed class NationalPupilDatabaseLearnerDataTransferObjectToNationalPupilDatabaseLearnerMapper : IMapper<NationalPupilDatabaseLearnerDataTransferObject, NationalPupilDatabaseLearner>
{
    public NationalPupilDatabaseLearner Map(NationalPupilDatabaseLearnerDataTransferObject input)
    {
        // Defensive null checks for required fields
        ArgumentNullException.ThrowIfNull(input);
        ArgumentException.ThrowIfNullOrEmpty(input.UPN);
        ArgumentException.ThrowIfNullOrEmpty(input.Forename);
        ArgumentException.ThrowIfNullOrEmpty(input.Surname);
        ArgumentNullException.ThrowIfNull(input.DOB);
        ArgumentNullException.ThrowIfNull(input.LocalAuthority);

        // Construct domain model using validated input
        return new NationalPupilDatabaseLearner(
            new UniquePupilNumber(input.UPN),
            new LearnerName(
                firstName: input.Forename,
                middleName: input.Middlenames ?? string.Empty,
                surname: input.Surname),
            new LearnerCharacteristics(
                birthDate: input.DOB.Value,
                gender: ParseGender(input.Sex, input.Gender)),
            new LocalAuthorityCode(input.LocalAuthority));
    }

    private static Gender ParseGender(string? sex, string? gender)
    {
        if (string.IsNullOrWhiteSpace(sex))
        {
            sex = gender;
        }

        return sex?.Trim().ToUpperInvariant() switch
        {
            "M" => Gender.Male,
            "F" => Gender.Female,
            "O" => Gender.Other,
            _ => Gender.Other // fall-back for unrecognized values
        };
    }
}
