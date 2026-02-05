using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;
using DfE.GIAP.Core.Search.Infrastructure.FurtherEducation.DataTransferObjects;

namespace DfE.GIAP.Core.Search.Infrastructure.FurtherEducation.Mappers;

/// <summary>
/// Maps a <see cref="FurtherEducationLearnerDataTransferObject"/> data transfer object
/// into a domain-level <see cref="FurtherEducationLearner"/> model.
/// </summary>
internal sealed class FurtherEducationSearchResultToLearnerMapper : IMapperWithResult<FurtherEducationLearnerDataTransferObject, FurtherEducationLearner>
{
    IMappedResult<FurtherEducationLearner> IMapperWithResult<FurtherEducationLearnerDataTransferObject, FurtherEducationLearner>.Map(FurtherEducationLearnerDataTransferObject input)
    {
        (bool isValid, string error) = ValidateRequest(input);

        if (!isValid)
        {
            return MappedResult<FurtherEducationLearner>.RequestError(error);
        }

        try
        {
            FurtherEducationLearner learner = new(
                new FurtherEducationLearnerIdentifier(input.ULN!),
                new LearnerName(input.Forename!, input.Surname!),
                new LearnerCharacteristics(
                    input.DOB!.Value,
                    new Sex(input.Sex)
                )
            );

            return MappedResult<FurtherEducationLearner>.Success(learner);
        }
        catch (Exception ex)
        {
            return MappedResult<FurtherEducationLearner>.MappingError(ex);
        }
    }



    private static (bool, string error) ValidateRequest(FurtherEducationLearnerDataTransferObject input)
    {
        if (input == null)
        {
            return (false, "input cannot be null");
        }

        if (string.IsNullOrEmpty(input.ULN))
        {
            return (false, "input.ULN cannot be null");
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

        return (true, string.Empty);
    }
}
