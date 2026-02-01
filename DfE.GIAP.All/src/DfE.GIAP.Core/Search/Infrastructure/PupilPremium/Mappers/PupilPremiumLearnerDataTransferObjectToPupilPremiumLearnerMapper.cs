using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;
using DfE.GIAP.Core.Search.Infrastructure.PupilPremium.DataTransferObjects;

namespace DfE.GIAP.Core.Search.Infrastructure.PupilPremium.Mappers;

public sealed class PupilPremiumLearnerDataTransferObjectToPupilPremiumLearnerMapper : IMapper<PupilPremiumLearnerDataTransferObject, PupilPremiumLearner>
{
    public PupilPremiumLearner Map(PupilPremiumLearnerDataTransferObject input)
    {
        // Defensive null checks for required fields
        ArgumentNullException.ThrowIfNull(input);
        ArgumentException.ThrowIfNullOrEmpty(input.UPN);
        ArgumentException.ThrowIfNullOrEmpty(input.Forename);
        ArgumentException.ThrowIfNullOrEmpty(input.Surname);
        ArgumentNullException.ThrowIfNull(input.DOB);
        ArgumentNullException.ThrowIfNull(input.LocalAuthority);

        // Construct domain model using validated input
        return new PupilPremiumLearner(
            new UniquePupilNumber(input.UPN),
            new LearnerName(input.Forename, input.Middlenames ?? string.Empty, input.Surname),
            new LearnerCharacteristics(input.DOB.Value, ParseGender(input.Sex, input.Gender)),
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
