using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;
using DfE.GIAP.Core.Search.Infrastructure.FurtherEducation.DataTransferObjects;

namespace DfE.GIAP.Core.Search.Infrastructure.FurtherEducation.Mappers;

/// <summary>
/// Maps a <see cref="FurtherEducationLearnerDataTransferObject"/> data transfer object
/// into a domain-level <see cref="FurtherEducationLearner"/> model.
/// </summary>
internal sealed class FurtherEducationSearchResultToLearnerMapper : IMapper<FurtherEducationLearnerDataTransferObject, FurtherEducationLearner>
{
    /// <summary>
    /// Converts a <see cref="FurtherEducationLearnerDataTransferObject"/> into a <see cref="FurtherEducationLearner"/>.
    /// </summary>
    /// <param name="input">The DTO representing a Further Education pupil.</param>
    /// <returns>A mapped domain model representing the learner.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="input"/> contains null or empty required fields.
    /// </exception>
    public FurtherEducationLearner Map(FurtherEducationLearnerDataTransferObject input)
    {
        // Defensive null checks for required fields
        ArgumentNullException.ThrowIfNull(input);
        ArgumentException.ThrowIfNullOrEmpty(input.ULN);
        ArgumentException.ThrowIfNullOrEmpty(input.Forename);
        ArgumentException.ThrowIfNullOrEmpty(input.Surname);
        ArgumentNullException.ThrowIfNull(input.DOB);

        // Construct domain model using validated input
        return new FurtherEducationLearner(
            new FurtherEducationLearnerIdentifier(input.ULN),
            new LearnerName(input.Forename, input.Surname),
            new LearnerCharacteristics(
                input.DOB.Value,
                new Sex(input.Sex)
            )
        );
    }
}
