using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;
using DfE.GIAP.Core.Search.Infrastructure.PupilPremium.DataTransferObjects;

namespace DfE.GIAP.Core.Search.Infrastructure.PupilPremium.Mappers;

internal sealed class PupilPremiumLearnerDataTransferObjectToPupilPremiumLearnerMapper : IMapper<PupilPremiumLearnerDataTransferObject, PupilPremiumLearner>
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

        DateTime dateOfBirth = input.DOB!.Value;

        // Construct domain model using validated input
        return new PupilPremiumLearner(
            new UniquePupilNumber(input.UPN),
            new LearnerName(
                firstName: input.Forename,
                middleName: input.Middlenames,
                surname: input.Surname),
            new LearnerCharacteristics(
                dateOfBirth,
                new Sex(input.Sex)),
            new LocalAuthorityCode(input.LocalAuthority));
    }
}
