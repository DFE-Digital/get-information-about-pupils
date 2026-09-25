using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;
using DfE.GIAP.Core.Search.Infrastructure.PupilPremium.DataTransferObjects;

namespace DfE.GIAP.Core.Search.Infrastructure.PupilPremium.Mappers;

internal sealed class PupilPremiumLearnerDataTransferObjectToPupilPremiumLearnerMapper : IMapperWithResult<PupilPremiumLearnerDataTransferObject, PupilPremiumLearner>
{
    public IMappedResult<PupilPremiumLearner> Map(PupilPremiumLearnerDataTransferObject input)
    {

        (bool isValid, string error) = ValidateRequest(input);

        if (!isValid)
        {
            return MappedResult<PupilPremiumLearner>.RequestError(error);
        }

        try
        {
            DateTime dateOfBirth = input.DOB!.Value;

            PupilPremiumLearner learner = new(
                        new UniquePupilNumber(input.UPN!),
                        new LearnerName(
                            firstName: input.Forename!,
                            middleName: input.Middlenames,
                            surname: input.Surname!),
                        new LearnerCharacteristics(
                            dateOfBirth,
                            new Sex(input.Sex)),
                        new LocalAuthorityCode(input.LocalAuthority!));

            return MappedResult<PupilPremiumLearner>.Success(learner);
        }
        catch (Exception ex)
        {
            return MappedResult<PupilPremiumLearner>.MappingError(ex);

        }
    }

    private static (bool, string error) ValidateRequest(PupilPremiumLearnerDataTransferObject input)
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
