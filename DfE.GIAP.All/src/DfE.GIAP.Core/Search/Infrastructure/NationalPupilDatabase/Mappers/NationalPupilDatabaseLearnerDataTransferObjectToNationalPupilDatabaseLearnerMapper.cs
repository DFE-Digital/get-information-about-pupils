using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;
using DfE.GIAP.Core.Search.Infrastructure.NationalPupilDatabase.DataTransferObjects;

namespace DfE.GIAP.Core.Search.Infrastructure.NationalPupilDatabase.Mappers;
internal sealed class NationalPupilDatabaseLearnerDataTransferObjectToNationalPupilDatabaseLearnerMapper
    : IMapperWithResult<NationalPupilDatabaseLearnerDataTransferObject, NationalPupilDatabaseLearner>
{
    public IMappedResult<NationalPupilDatabaseLearner> Map(NationalPupilDatabaseLearnerDataTransferObject input)
    {
        (bool isValid, string error) = ValidateRequest(input);

        if (!isValid)
        {
            return MappedResult<NationalPupilDatabaseLearner>.RequestError(error);
        }

        try
        {

            // sex is not guanteed on NPD records, so we fallback so we fallback to {pupil}.Gender
            Sex sex = new(string.IsNullOrWhiteSpace(input.Sex) ? input.Gender : input.Sex);

            NationalPupilDatabaseLearner learner = new(
                new UniquePupilNumber(input.UPN!),
                new LearnerName(
                    firstName: input.Forename!,
                    middleName: input.Middlenames,
                    surname: input.Surname!),
                new LearnerCharacteristics(
                    birthDate: input.DOB!.Value,
                    sex: sex),
                new LocalAuthorityCode(input.LocalAuthority!));

            return MappedResult<NationalPupilDatabaseLearner>.Success(learner);
        }
        catch (Exception ex)
        {
            return MappedResult<NationalPupilDatabaseLearner>.MappingError(ex);
        }
    }

    private static (bool, string error) ValidateRequest(NationalPupilDatabaseLearnerDataTransferObject input)
    {
        if (input == null)
        {
            return (false, "input cannot be null");
        }

        if (string.IsNullOrEmpty(input.UPN))
        {
            return (false, "input.UPN cannot be null");
        }

        if (string.IsNullOrEmpty(input.Forename))
        {
            return (false, "input.Forename cannot be null");
        }

        if (string.IsNullOrEmpty(input.Surname))
        {
            return (false, "input.Surname cannot be null");
        }

        if (input.DOB == null)
        {
            return (false, "input.DOB cannot be null");
        }

        if (input.LocalAuthority == null)
        {
            return (false, "input.LocalAuthority cannot be null");
        }

        return (true, string.Empty);
    }


}
