using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
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
                middleName: input.Middlenames,
                surname: input.Surname),
            new LearnerCharacteristics(
                birthDate: input.DOB.Value,
                // sex is not guanteed on NPD records, so we fallback so we fallback to {pupil}.Gender
                sex: new Sex(string.IsNullOrWhiteSpace(input.Sex) ? input.Gender : input.Sex)),
            new LocalAuthorityCode(input.LocalAuthority));
    }
}
